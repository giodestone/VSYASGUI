using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vintagestory.API.Server;
using VSYASGUI_CommonLib;
using VSYASGUI_CommonLib.RequestObjects;
using VSYASGUI_CommonLib.RequestObjects.FileRequests;
using VSYASGUI_CommonLib.ResponseObjects;

namespace VSYASGUI_Mod
{
    /// <summary>
    /// Manages the actual HTTP API provided by the mod.
    /// </summary>
    internal class HttpApi
    {
        ICoreServerAPI _Api = null;
        HttpListener _HttpListener = null;
        Config _Config = null;
        LogCache _LogCache = null;
        Guid _InstanceGuid;
        CpuLoadCalc _CpuLoadCalc;
        
        Task _AcceptLoop;
        CancellationTokenSource _AcceptLoopCancellationTokenSource;

        public HttpApi(ICoreServerAPI api, Config config, LogCache logCache, Guid instanceGuid, CpuLoadCalc cpuLoadCalc)
        {
            _Config = config;
            _Api = api;
            _LogCache = logCache;
            _InstanceGuid = instanceGuid;
            _CpuLoadCalc = cpuLoadCalc;
        }
        
        ~HttpApi()
        {
            try
            {
                _AcceptLoopCancellationTokenSource.Cancel();
            }
            catch
            { }
        }

        /// <summary>
        /// Start the server.
        /// </summary>
        /// <exception cref="HttpListenerException">Throws various exceptions if the <see cref="HttpListener"/> fails to start.</exception>
        public void Start()
        {
            try
            {
                _HttpListener = new HttpListener();
                _HttpListener.Prefixes.Add(_Config.BindURL);
                _HttpListener.Start();

                _AcceptLoopCancellationTokenSource = new CancellationTokenSource();
                _AcceptLoop = AcceptLoop(_AcceptLoopCancellationTokenSource.Token);
                _Api.Logger.Notification($"VSYASGUI_Mod-Mod: API now ready at {_Config.BindURL}");
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Main loop for any requests coming in. Launches a new instance of <see cref="HandleRequest(CancellationToken, HttpListenerContext)"/> for any recieved request that can be correctly interpreted.
        /// </summary>
        /// <returns>Task which does not have to be run synchronously.</returns>
        private async Task AcceptLoop(CancellationToken cancellationToken)
        {
            if (_HttpListener == null)
                return;

            while (_HttpListener.IsListening || !cancellationToken.IsCancellationRequested)
            {
                HttpListenerContext httpListenerContext;
                try
                {
                    httpListenerContext = await _HttpListener.GetContextAsync().ConfigureAwait(false);
                }
                catch (HttpListenerException)
                {
                    break; // listener stopped
                }
                catch (ObjectDisposedException)
                {
                    break; // disposed during shutdown
                }

                //Task.Run(_ => HandleRequest(cancellationToken, httpListenerContext), httpListenerContext
                _ = HandleRequest(cancellationToken, httpListenerContext);
            }
        }

        /// <summary>
        /// Handles a request.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task HandleRequest(CancellationToken cancellationToken, HttpListenerContext context)
        {
            if (!IsApiKeyMatching(context))
            {
                context.Response.StatusCode = 401;
                WriteJsonToResponse(context, ResponseFactory.MakeErrorUnauthorised());
                return;
            }

            if (context.Request.Url == null)
            {
                context.Response.StatusCode = 400;
                WriteJsonToResponse(context, ResponseFactory.MakeErrorBadRequest());
                return;
            }

            switch (context.Request.Url.AbsolutePath.TrimEnd('/'))
            {
                case "/players-online":
                    await SendPlayersOnlineResponse(context);
                    break;
                case "/players":
                    await SendPlayerOverviewResponse(context);
                    break;
                case "/console":
                    await SendConsoleResponse(context);
                    break;
                case "/command":
                    await SendCommandResponse(context);
                    break;
                case "/statistics":
                    await SendStatisticsResponse(context);
                    break;
                case "/backup-download":
                    await SendBackupFileResponse(context); // TEMP
                    break;
                case "/save-backups":
                    await SendDirectoryInfo(context, "Backups");
                    break;
                case "/":
                default:
                    await SendConnectionCheckResponse(context);
                    break;
            }
        }

        private async Task SendDirectoryInfo(HttpListenerContext context, string directoryName)
        {
            DirectoryInfo directoryInfo;
            try
            {
                directoryInfo = new DirectoryInfo(_Api.DataBasePath + Path.DirectorySeparatorChar + directoryName);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                WriteJsonToResponse(context, ResponseFactory.MakeErrorBadRequest("Unable to find directory."));
                return;
            }

            List<string> fileNames = new List<string>();

            foreach (var file in directoryInfo.EnumerateFiles())
            {
                fileNames.Add(file.Name);
            }

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            WriteJsonToResponse(context, ResponseFactory.MakeDirectoryResponse(fileNames));
        }

        /// <summary>
        /// Responds with a file.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="filePathRelativeToData">Relative path to file, do not include leading / .</param>
        /// <returns></returns>
        private async Task SendBackupFileResponse(HttpListenerContext context)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(_Api.DataBasePath + Path.DirectorySeparatorChar + "Backups");

            WorldDownloadRequest? request = null;

            try
            {
                // TODO: Add cancellation token.
                request = await JsonSerializer.DeserializeAsync<WorldDownloadRequest>(context.Request.InputStream);
            } 
            catch (Exception e)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                WriteJsonToResponse(context, ResponseFactory.MakeErrorBadRequest("Failed to read request: " + e.Message));
                return;
            }

            // TODO: I think just sending a UUID for each file is much safer, as this is a silly amount of injection checking and it probably doesn't cover all bases...
            if (request == null || string.IsNullOrEmpty(request.FileName) || string.IsNullOrWhiteSpace(request.FileName) || request.FileName.Contains(Path.PathSeparator) || request.FileName.Contains(Path.DirectorySeparatorChar) || request.FileName.ContainsAny(Path.GetInvalidFileNameChars()) || request.FileName.Contains('*'))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                WriteJsonToResponse(context, ResponseFactory.MakeErrorBadRequest("Bad file name."));
                return;
            }

            // TODO: This is a file system operation... which could stall. This should become its own task that we wait for.
            if (!directoryInfo.EnumerateFiles().Any(f => f.Name == request.FileName))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                WriteJsonToResponse(context, ResponseFactory.MakeErrorBadRequest("File does not exist."));
                return;
            }

            // Confirm file can be accessed.
            FileInfo? requestedFileInfo = null;
            try
            {
                requestedFileInfo = new FileInfo(_Api.DataBasePath + Path.DirectorySeparatorChar + request.FileName);
            }
            catch (UnauthorizedAccessException uex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                WriteJsonToResponse(context, ResponseFactory.MakeErrorBadRequest("Unable to provide file due to access/permission issue: " + uex.Message));
                return;
            }
            catch (Exception e)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                WriteJsonToResponse(context, ResponseFactory.MakeErrorBadRequest("Unable to provide file due to an exception: " + e.Message));
                return;
            }

            // Confirm file exists.
            if (!requestedFileInfo.Exists)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                WriteJsonToResponse(context, ResponseFactory.MakeErrorBadRequest("Unable to provide file as it doesn't exist."));
                return;
            }

            try
            {
                var response = context.Response;
                using (FileStream fs = File.OpenRead(requestedFileInfo.FullName))
                {
                    string filename = Path.GetFileName(requestedFileInfo.FullName);
                    //response is HttpListenerContext.Response...
                    response.ContentLength64 = fs.Length;
                    response.SendChunked = false;
                    response.ContentType = System.Net.Mime.MediaTypeNames.Application.Octet;
                    response.AddHeader("Content-disposition", "attachment; filename=" + filename);

                    byte[] buffer = new byte[64 * 1024];
                    int read;
                    using (BinaryWriter bw = new BinaryWriter(response.OutputStream))
                    {
                        while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            bw.Write(buffer, 0, read);
                            bw.Flush(); //seems to have no effect
                        }

                        bw.Close();
                    }

                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.StatusDescription = "OK";
                    response.OutputStream.Close();
                }
            }
            catch (Exception e)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                WriteJsonToResponse(context, ResponseFactory.MakeErrorBadRequest("Unable to send file due to an exception: " + e.Message));
                return;
            }



            //var fileOutPath = Path.GetTempPath() + Guid.NewGuid().ToString() + ".txt";
            //FileStream? tempBase64FileStream = null;

            //try
            //{
            //    tempBase64FileStream = File.Create(fileOutPath);
            //}
            //catch (Exception e)
            //{
            //    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            //    ResponseFactory.MakeErrorBadRequest("Unable to provide file due to an exception when creating a temporary intermediary file: " + e.Message);
            //    return;
            //}

            //using (var cs = new CryptoStream(tempBase64FileStream, new ToBase64Transform(),
            //                                         CryptoStreamMode.Write))
            //{
            //    using (var fi = File.Open(filein, FileMode.Open))
            //    {
            //        fi.CopyTo(cs);
            //    }
            //}



            //// Convert file to base64 string.
            //string fileString = string.Empty;

            //try
            //{
            //    var fileBytes = await File.ReadAllBytesAsync(requestedFileInfo.FullName);
            //    fileString = Convert.ToBase64String(fileBytes);
            //}
            //catch (Exception e)
            //{
            //    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            //    ResponseFactory.MakeErrorBadRequest("Unable to provide file due to an exception: " + e.Message);
            //    return;
            //}

            //SHA256.HashData()


            //string worldBackupFileName = "vsyasguimod_world_download_" + DateTime.Now.ToString("hh_mm_ss_dd_MM_YYYY") + ".temp";

            //await RunOnApiThread(() => _Api.InjectConsole("/genbackup " + worldBackupFileName));

            //string fullPathToBackedUpFile = _Api.DataBasePath + "/Backups/" + worldBackupFileName;

            //FileInfo fileInfo = new FileInfo(fullPathToBackedUpFile);
            //var lastFileWriteTime = fileInfo.LastWriteTime;

            //while (fileInfo.LastWriteTime)


            // make response that includes details on how to download it
            // give URL where the response may be gotten.
            // up to client to begin download.
            // link is terminated after download is completed.

        }

        /// <summary>
        /// Populate the response with <see cref="PlayerOverviewResponse"/>.
        /// </summary>
        private async Task SendPlayerOverviewResponse(HttpListenerContext context)
        {
            List<PlayerOverview>? playerOverviews = null;

            await RunOnApiThread(() =>
            {
                // TODO: Improve this: make only e.g. 10 players run at a time, as otherwise this operation may take too long.
                
                playerOverviews = new List<PlayerOverview>(_Api.Server.Players.Length);

                // Cross reference the ConnectionState with players that have previously connected during this run of the server.
                foreach (var player in _Api.Server.Players)
                {
                    // Consolodate player groups into a comma seperated list.
                    string playerGroups = string.Empty;
                    foreach (var group in player.Groups)
                    {
                        playerGroups += group.GroupName + ", ";
                    }
                    playerGroups = playerGroups.Trim();
                    playerGroups = playerGroups.Trim(',');


                    // Player name can be null if offline, in which case request the stored data.
                    string playerName = player.PlayerName;
                    if (player.ConnectionState == EnumClientState.Offline)
                    {
                        if (_Api.PlayerData.PlayerDataByUid.ContainsKey(player.PlayerUID))
                            playerName = _Api.PlayerData.PlayerDataByUid[player.PlayerUID].LastKnownPlayername;
                    }

                    // Get data from playerdata, which can be null and crash the server (bad).
                    string lastKnownName = string.Empty;
                    string firstJoinDate = string.Empty;
                    string lastJoinDate = string.Empty;
                    
                    if (_Api.PlayerData.PlayerDataByUid.ContainsKey(player.PlayerUID))
                    {
                        lastKnownName = _Api.PlayerData.PlayerDataByUid[player.PlayerUID].LastKnownPlayername;
                        firstJoinDate = _Api.PlayerData.PlayerDataByUid[player.PlayerUID].FirstJoinDate;
                        lastJoinDate = _Api.PlayerData.PlayerDataByUid[player.PlayerUID].LastJoinDate;
                    }


                    playerOverviews.Add(new PlayerOverview()
                    {
                        Name = playerName,
                        Groups = playerGroups,
                        ConnectionState = player.ConnectionState.ToString(),
                        PlayerUid = player.PlayerUID,
                        LastKnownName = lastKnownName,
                        FirstJoinDate = firstJoinDate,
                        LastJoinDate = lastJoinDate,
                    });
                }
            });

            if (!WriteJsonToResponse(context, ResponseFactory.MakePlayerOverviewResponse(playerOverviews)) )
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            context.Response.StatusCode = (int)HttpStatusCode.OK;
        }

        /// <summary>
        /// Populates the response with <see cref="ServerStatisticsResponse"/>.
        /// </summary>
        private async Task SendStatisticsResponse(HttpListenerContext context)
        {

            double cpuUsagePercentage = -1;
            long memUsageBytes = -1;
            int secondsUptime = -1;
            int totalWorldPlaytime = -1;
            int onlinePlayers = -1;

            await RunOnApiThread(() =>
            {
                Process currentProcess = Process.GetCurrentProcess();
                memUsageBytes = currentProcess.WorkingSet64;
                cpuUsagePercentage = _CpuLoadCalc.ProcessorUsagePercentage;

                secondsUptime = _Api.Server.ServerUptimeSeconds;
                totalWorldPlaytime = _Api.Server.TotalWorldPlayTime;
                onlinePlayers = _Api.Server.Players.Count(p => p.ConnectionState == EnumClientState.Playing);
            });

            var serverStatisticsResponse = ResponseFactory.MakeServerStatisticsResponse(cpuUsagePercentage, memUsageBytes, secondsUptime, totalWorldPlaytime, onlinePlayers);
            
            if (!WriteJsonToResponse(context, serverStatisticsResponse))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            context.Response.StatusCode = (int)HttpStatusCode.OK;
        }

        /// <summary>
        /// Populate a <see cref="ConsoleCommandResponse"/>.
        /// </summary>
        private async Task SendCommandResponse(HttpListenerContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            string requestBody = await ReadRequestContents(context);
            CommandRequest? request = DeserialiseIntoRequestObject<CommandRequest>(requestBody);

            if (request == null)
                return;

            await RunOnApiThread(() => { _Api.InjectConsole(request.Command); });         

            context.Response.StatusCode = (int)HttpStatusCode.OK;
        }

        /// <summary>
        /// Run an action on the main API thread for when the return value does not matter.<br/>
        /// Wrapper for <see cref="RunOnApiThread{T}(Func{T})"/>.
        /// </summary>
        /// <param name="action">The action to run.</param>
        private Task RunOnApiThread(Action action)
        {
            return RunOnApiThread<bool>(() => { action(); return true; });
        }

        /// <summary>
        /// Creates a task which will run on the main thread from another thread (e.g. async), as the Vintage Story API is not threadsafe.
        /// </summary>
        /// <typeparam name="T">Return value type of the function to run.</typeparam>
        /// <param name="functionToRun">The function to run on the main thread.</param>
        private Task<T> RunOnApiThread<T>(Func<T> functionToRun)
        {
            TaskCompletionSource<T> taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

            _Api.Event.EnqueueMainThreadTask(() =>
            {
                try
                {
                    taskCompletionSource.TrySetResult(functionToRun());
                }
                catch (Exception e)
                {
                    taskCompletionSource.SetException(e);
                }
            },
            "VSYASGUI_Mod-Mod");

            return taskCompletionSource.Task;
        }

        /// <summary>
        /// Responds with the player details of the currently online player.
        /// </summary>
        /// <param name="context">The response.</param>
        /// <returns></returns>
        private async Task SendPlayersOnlineResponse(HttpListenerContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            List<PlayerDetails> players = new List<PlayerDetails>(0);
            await RunOnApiThread(() => players = _Api.Server.Players.Where(p => p.ConnectionState != EnumClientState.Offline).Select(p => PlayerDetails.FromServerPlayer(p)).ToList());              
            WriteJsonToResponse(context, players);
        }

        /// <summary>
        /// Responds with the <see cref="ConnectionCheckResponse"/>.
        /// </summary>
        /// <param name="context">The response to reply to.</param>
        private async Task SendConnectionCheckResponse(HttpListenerContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            WriteJsonToResponse(context, ResponseFactory.MakeConnectionCheckResponse(this._InstanceGuid));
        }

        /// <summary>
        /// Respond with console details.
        /// </summary>
        private async Task SendConsoleResponse(HttpListenerContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            string strmContents = await ReadRequestContents(context);

            ConsoleRequest? request = DeserialiseIntoRequestObject<ConsoleRequest>(strmContents);

            if (request == null)
                return;

            context.Response.StatusCode = 200;

            List<string> logLines = null;
            long lineFrom = -1;
            long lineTo = -1;

            // Would need to guarantee the LogCache is thead safe, which seems like it does not need to be for now.
            await RunOnApiThread(() => _LogCache.GetLog(request.LineFrom, out logLines, out lineFrom, out lineTo));
            
            WriteJsonToResponse(context, ResponseFactory.MakeConsoleEntriesResponse(logLines, lineFrom, lineTo, _InstanceGuid));
        }
        
        /// <summary>
        /// Deserialise the given string into a response object.
        /// </summary>
        /// <remarks>
        /// The expected size of the obect is rather small. If parsing in large objects, consider using <see cref="JsonSerializer.DeserializeAsync{TValue}(System.IO.Stream, JsonSerializerOptions?, System.Threading.CancellationToken)"/>.
        /// <br/>
        /// Consider <typeparamref name="T"/> carefully: it is possible for a base class to incorrectly be considered.
        /// </remarks>
        /// <typeparam name="T">Type of request.</typeparam>
        /// <param name="json">Processed string into json.</param>
        /// <returns>The object if successful</returns>
        private T DeserialiseIntoRequestObject<T>(string json) where T : RequestBase
        {
            T? request = null;
            try
            {
                request = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions() { IncludeFields = true });
            }
            catch
            {
                return null;
            }

            return request;
        }

        /// <summary>
        /// Read the contents of the request as provided by the <paramref name="context"/>.
        /// </summary>
        /// <returns>Request contents. <see cref="string.Empty"/> if Error occurs when reading (or the request contents are empty).</returns>
        private async Task<string> ReadRequestContents(HttpListenerContext context)
        {
            string strmContents = string.Empty;
            try
            {
                byte[] bytes = new byte[CommonVariables.MaxRequestSize];
                int strRead = await context.Request.InputStream.ReadAsync(bytes, 0, CommonVariables.MaxRequestSize);

                // Convert byte array to a text string.
                strmContents = context.Request.ContentEncoding.GetString(bytes, 0, strRead);
            }
            catch
            {
                return strmContents;
            }

            return strmContents;
        }

        /// <summary>
        /// Returns true if the recieved API key in the header matches <see cref="Config.ApiKey"/>.
        /// </summary>
        private bool IsApiKeyMatching(HttpListenerContext context)
        {
            var recievedKey = context.Request.Headers[CommonVariables.RequestHeaderApiKeyName];

            if (recievedKey == null)
                return false;

            return string.Equals(recievedKey, _Config.ApiKey, StringComparison.Ordinal);
        }

        /// <summary>
        /// Writes the <paramref name="objToJsonise"/> to the <see cref="HttpListenerContext.Response"/> as a UTF8 string.
        /// 
        /// If fails (either due to writing stream failure, or JSON serialisation failure), will return false and log exception to the <see cref="IServerConfig"/>'s <see cref="Vintagestory.API.Common.ILogger"/>.
        /// </summary>
        /// <param name="objToJsonise"></param>
        private bool WriteJsonToResponse(HttpListenerContext context, object objToJsonise)
        {
            try
            {
                JsonSerializerOptions options = new JsonSerializerOptions() { IncludeFields = true };
                string serialisedObj = JsonSerializer.Serialize(objToJsonise, options);
                byte[] byteSerialisedObj = Encoding.UTF8.GetBytes(serialisedObj);
                context.Response.ContentType = System.Net.Mime.MediaTypeNames.Application.Json;
                context.Response.OutputStream.Write(byteSerialisedObj, 0, byteSerialisedObj.Length);
                context.Response.Close();
                return true;
            }
            catch (Exception e)
            {
                _Api.Logger.Error($"VSYASGUI_Mod-Mod: Failed to write response to stream.");
                _Api.Logger.LogException(Vintagestory.API.Common.EnumLogType.Error, e);
                return false;
            }
        }
    }
}
