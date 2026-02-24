using System.Net.Http;
using System.Text.Json;
using VSYASGUI_CommonLib;
using VSYASGUI_CommonLib.RequestObjects;
using VSYASGUI_CommonLib.ResponseObjects;

namespace VSYASGUI_WFP_App.MVVM.Models
{
    /// <summary>
    /// Represents a connection to the VS Server.
    /// </summary>
    internal class ApiConnection
    {
        internal struct RequestResult
        {
            public Error Error;
            public string HttpContent;

            public RequestResult(Error error, string httpContent)
            {
                Error = error;
                HttpContent = httpContent;
            }
        }

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


        ///// <summary>
        ///// Check the connection to the specified endpoint using the api key.
        ///// </summary>
        ///// <param name="cancellationToken">Token for cancellation.</param>
        ///// <returns>
        ///// <list type="bullet">
        ///// <item><see cref="Error.Ok"/> if status 200 is returned. </item>
        ///// <item><see cref="Error.Cancelled"/> if cancelled. </item>
        ///// <item><see cref="Error.Connection"/> if unable to connect, or initialise request. </item> 
        ///// <item><see cref="Error.Unauthorised"/> if 401 is returned. </item>
        ///// <item><see cref="Error.General"/> if a different failure occurred (probably due to an OS error). </item>
        ///// </list>
        ///// </returns>
        //public async Task<Error> CheckConnection(CancellationToken cancellationToken)
        //{
        //    ConnectionRequest connectionRequest = new() { ApiKey = _ApiKey };
        //    return await SendHttpRequest(connectionRequest, cancellationToken);
        //}

        

        //public async Task<ApiResponse<ConsoleEntriesResponse>> GetConsoleValues(CancellationToken cancellationToken)
        //{
        //    ConsoleRequest request = new ConsoleRequest() { ApiKey = Config.Instance.CurrentApiKey };
        //    var response = await SendHttpRequest(request, cancellationToken);

        //    ConsoleEntriesResponse? serialisedObject = null;

        //    try
        //    {
        //        serialisedObject = JsonSerializer.Deserialize<ConsoleEntriesResponse>(response.HttpContent.ReadAsStream());
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ApiResponse<ConsoleEntriesResponse>(Error.Deserialisation, null);
        //    }

        //    return new ApiResponse<ConsoleEntriesResponse>(Error.Ok, serialisedObject);

        //}

        public async Task<ApiResponse<TExpectedResponse>> RequestApiInfo<TExpectedResponse>(RequestBase request, CancellationToken cancellationToken) where TExpectedResponse : ResponseBase, new()
        {
            request.ApiKey = Config.Instance.CurrentApiKey;
            var response = await SendHttpRequest(request, cancellationToken);

            if (response.Error != Error.Ok)
                return new ApiResponse<TExpectedResponse>(response.Error, null);

            TExpectedResponse? serialisedObject = null;

            TExpectedResponse r = new(); // Sureley there is a better wayt to do this.
            if (response.HttpContent == null && r.ExpectsResponse)
                return new ApiResponse<TExpectedResponse>(response.Error, null);

            try
            {
                serialisedObject = JsonSerializer.Deserialize<TExpectedResponse>(response.HttpContent, new JsonSerializerOptions() { IncludeFields = true });
            }
            catch (Exception ex)
            {
                return new ApiResponse<TExpectedResponse>(Error.Deserialisation, null);
            }

            return new ApiResponse<TExpectedResponse>(Error.Ok, serialisedObject);

        }

        private async Task<RequestResult> SendHttpRequest(RequestBase request, CancellationToken cancellationToken)
        {
            HttpResponseMessage? response = null;

            try
            {
                // TODO: fix this to include the api key in the header
                HttpContent c = SerialiseObject(request);
                c.Headers.Add(CommonVariables.RequestHeaderApiKeyName, request.ApiKey);
                response = await _Client.PostAsync(_EndpointUri + request.Address, c, cancellationToken);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException httpRequestException)
            {
                if (httpRequestException.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return new RequestResult(Error.Unauthorised, null);
                return new RequestResult(Error.General, null);
            }
            catch (TaskCanceledException cancelledException)
            {
                return new RequestResult(Error.Cancelled, null);
            }
            catch (Exception e)
            {
                return new RequestResult(Error.General, null);
            }

            if (response == null)
            {
                return new RequestResult(Error.Connection, null);
            }

            string contentString = await response.Content.ReadAsStringAsync();

            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    return new RequestResult(Error.Ok, contentString);
                case System.Net.HttpStatusCode.Unauthorized:
                    return new RequestResult(Error.Unauthorised, contentString);
                default:
                    return new RequestResult(Error.General, contentString);
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
                serialised = JsonSerializer.Serialize(obj, new JsonSerializerOptions { IncludeFields = true });
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
