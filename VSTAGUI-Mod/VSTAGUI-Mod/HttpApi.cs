using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Vintagestory.API.Server;
using VSYASGUI_CommonLib;
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

        public HttpApi(ICoreServerAPI api, Config config, LogCache logCache)
        {
            _Config = config;
            _Api = api;
            _LogCache = logCache;
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

                _HttpListener.BeginGetContext(OnHttp, this);
                _Api.Logger.Notification($"VSYASGUI-Mod: API now ready at {_Config.BindURL}");
            }
            catch
            {
                throw;
            }
        }

        private void OnHttp(IAsyncResult ar)
        {
            var context = _HttpListener.EndGetContext(ar);
            _HttpListener.BeginGetContext(OnHttp, this);
            HandleRequest(context);

            //if (_HttpListener == null || !_HttpListener.IsListening)
            //    return;


        }

        private void HandleRequest(HttpListenerContext context)
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
                    SendPlayersOnlineResponse(context);
                    break;
                case "/console":
                    SendConsoleResponse(context);
                    break;
                case "/":
                    SendConnectionCheckResponse(context);
                    break;
                default:
                    context.Response.StatusCode = 418;
                    WriteJsonToResponse(context, ResponseFactory.MakeErrorBadRequest());
                    break;
            }
        }

        private void SendPlayersOnlineResponse(HttpListenerContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            var players = _Api.Server.Players.Where(p => p.ConnectionState != EnumClientState.Offline).Select(p => PlayerDetails.FromServerPlayer(p)).ToList();
            WriteJsonToResponse(context, players);
        }

        private static void SendConnectionCheckResponse(HttpListenerContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;
        }

        /// <summary>
        /// Send console details.
        /// </summary>
        private void SendConsoleResponse(HttpListenerContext context)
        {
            context.Response.StatusCode = 200;
            WriteJsonToResponse(context, ResponseFactory.MakeConsoleEntriesResponse(_LogCache.GetLog()));
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
                string serialisedObj = JsonSerializer.Serialize(objToJsonise);
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
