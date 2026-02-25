using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vintagestory.API.Server;
using VSYASGUI_CommonLib;
using VSYASGUI_CommonLib.RequestObjects;
using VSYASGUI_CommonLib.ResponseObjects;
using VSYASGUI_Mod;

namespace VSYASGUI
{
    internal class HttpApi
    {
        ICoreServerAPI _Api = null;
        HttpListener _HttpListener = null;
        Config _Config = null;
        LogCache _LogCache = null;
        Guid _InstanceGuid;
        
        Task _AcceptLoop;
        CancellationTokenSource _AcceptLoopCancellationTokenSource;

        public HttpApi(ICoreServerAPI api, Config config, LogCache logCache, Guid instanceGuid)
        {
            _Config = config;
            _Api = api;
            _LogCache = logCache;
            _InstanceGuid = instanceGuid;
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
                _Api.Logger.Notification($"VSYASGUI-Mod: API now ready at {_Config.BindURL}");
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

                //Task.Run(_ => HandleRequest(cancellationToken, httpListenerContext), httpListenerContext);

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
                case "/console":
                    await SendConsoleResponse(context);
                    break;
                case "/command":
                    await SendCommandResponse(context);
                    break;
                case "/":
                    await SendConnectionCheckResponse(context);
                    break;
                default:
                    context.Response.StatusCode = 200;
                    WriteJsonToResponse(context, ResponseFactory.MakeConnectionCheckResponse(_InstanceGuid));
                    break;
            }
        }

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
        /// Function to run on the main thread from another thread (e.g. async), as the Vintage Story API is not threadsafe.
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
            "VSYASGUI-Mod");

            return taskCompletionSource.Task;
        }

        private async Task SendPlayersOnlineResponse(HttpListenerContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            var players = _Api.Server.Players.Where(p => p.ConnectionState != EnumClientState.Offline).Select(p => PlayerDetails.FromServerPlayer(p)).ToList();
            WriteJsonToResponse(context, players);
        }

        private async Task SendConnectionCheckResponse(HttpListenerContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;
        }

        /// <summary>
        /// Send console details.
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
        /// <returns>Request contents. <see cref="string.Empty"/> if error occurs when reading (or the request contents are empty).</returns>
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
                context.Response.ContentType = "application/json";
                context.Response.OutputStream.Write(byteSerialisedObj, 0, byteSerialisedObj.Length);
                context.Response.Close();
                return true;
            }
            catch (Exception e)
            {
                _Api.Logger.Error($"VSYASGUI-Mod: Failed to write response to stream.");
                _Api.Logger.LogException(Vintagestory.API.Common.EnumLogType.Error, e);
                return false;
            }
        }
    }
}
