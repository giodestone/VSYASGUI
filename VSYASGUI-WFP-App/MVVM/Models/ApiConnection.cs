using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VSYASGUI_CommonLib.RequestObjects;

namespace VSYASGUI_WFP_App.MVVM.Models
{
    /// <summary>
    /// Represents a connection to the VS Server.
    /// </summary>
    internal class ApiConnection
    {
        public static ApiConnection? Instance { get; protected set; }
        
        HttpClient _Client;

        string _EndpointUri = string.Empty; // Top level URI, e.g. http://127.0.0.1:8080/
        string _ApiKey = string.Empty;

        protected ApiConnection(string endpointUri, string apiKey)
        {
            _EndpointUri = endpointUri;
            _ApiKey = apiKey;
            _Client = new HttpClient();
        }


        /// <summary>
        /// Setup the required connection variables.
        /// </summary>
        /// <param name="endpointUri"></param>
        /// <param name="apiKey"></param>
        public static void SetupConnection(string endpointUri, string apiKey)
        {
            Instance = new ApiConnection(endpointUri, apiKey);
        }


        /// <summary>
        /// Check the connection to the specified endpoint using the api key.
        /// </summary>
        /// <param name="cancellationToken">Token for cancellation.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><see cref="Error.Ok"/> if connection is okay. </item>
        /// <item><see cref="Error.Cancelled"/> if cancelled. </item>
        /// <item><see cref="Error.Connection"/> if unable to connect, or initialise request. </item> 
        /// <item><see cref="Error.Unauthorised"/> if 401 is returned. </item>
        /// <item><see cref="Error.General"/> if a different failure occurred (probably due to an OS error). </item>
        /// </list>
        /// </returns>
        public async Task<Error> CheckConnection(CancellationToken cancellationToken)
        {
            ConnectionRequest connectionRequest = new() { ApiKey  = _ApiKey };

            HttpResponseMessage? response = null;

            try
            {
                response = await _Client.PostAsync(_EndpointUri, SerialiseObject(connectionRequest), cancellationToken);
            }
            catch (HttpRequestException httpRequestException)
            {
                if (httpRequestException.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return Error.Unauthorised;
                return Error.General;
            }
            catch (TaskCanceledException cancelledException)
            {
                return Error.Cancelled;
            }
            catch (Exception e)
            {
                return Error.General;
            }

            if (response == null)
            { 
                return Error.Connection;
            }

            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    return Error.Ok;
                case System.Net.HttpStatusCode.Unauthorized:
                    return Error.Unauthorised;
                default:
                    return Error.General;
            }
        }

        /// <summary>
        /// Serialises an object to JSON.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>JSON. Empty if failed to serialise.</returns>
        private StringContent SerialiseObject(object obj)
        {
            string serialised = string.Empty;
            try
            {
                serialised = JsonSerializer.Serialize(obj);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: Failed to serialise requested object.");
                Console.WriteLine(e.Message);
            }

            return new StringContent(serialised);
        }

    }
}
