using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using VSYASGUI_CommonLib;
using VSYASGUI_CommonLib.RequestObjects;
using VSYASGUI_CommonLib.ResponseObjects;
using static System.Net.WebRequestMethods;

namespace VSYASGUI_WFP_App.MVVM.Models
{
    /// <summary>
    /// Represents a connection to the VS Server.
    /// </summary>
    internal class ApiConnection
    {
        /// <summary>
        /// Content of the request along with the <see cref="Error"/>.
        /// </summary>
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

        string _EndpointUri = string.Empty; // Top level URI to send the requests to, e.g. http://127.0.0.1:8080/

        protected ApiConnection(string endpointUri)
        {
            _EndpointUri = endpointUri;
            _Client = new HttpClient();
        }


        /// <summary>
        /// Setup the required connection variables.
        /// </summary>
        public static void SetupConnection(string endpointUri)
        {
            Instance = new ApiConnection(endpointUri);
        }

        /// <summary>
        /// Requests information from the API.
        /// </summary>
        /// <typeparam name="TExpectedResponse">The type of the expected response. This is checked.</typeparam>
        /// <param name="request">The request to send.</param>
        /// <param name="cancellationToken">The token this can be cancelled with.</param>
        /// <returns>A task that can be cancelled.</returns>
        public async Task<ApiResponse<TExpectedResponse>> RequestApiInfo<TExpectedResponse>(RequestBase request, CancellationToken cancellationToken) where TExpectedResponse : ResponseBase, new()
        {
            request.ApiKey = Config.Instance.CurrentApiKey;
            var response = await SendHttpRequest(request, cancellationToken);

            if (response.Error != Error.Ok)
                return new ApiResponse<TExpectedResponse>(response.Error, null);

            TExpectedResponse? serialisedObject = null;

            TExpectedResponse r = new(); // Surely there is a better way to do this.
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

        /// <summary>
        /// Send a request to the server.
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A cancellable task with the result of the response, and the returned details (if applicable).</returns>
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
                else if (httpRequestException.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    return new RequestResult(Error.BadRequest, null);
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

            if (response.Content.Headers.ContentType?.MediaType == System.Net.Mime.MediaTypeNames.Application.Octet)
            {
                // this is a file to be downloaded - must be handled appropriately.
                await HandleFileResponse(response);
                return new RequestResult(Error.Ok, null);
            }
            else if (response.Content.Headers.ContentType?.MediaType == System.Net.Mime.MediaTypeNames.Application.Json)
            {
                // This can be parsed into a response type.
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
            else
            {
                return new RequestResult(Error.BadType, null);
            }
        }

        /// <summary>
        /// Handles a file response.
        /// </summary>
        /// <param name="response"></param>
        /// <exception cref="NotImplementedException"></exception>
        private async Task HandleFileResponse(HttpResponseMessage response)
        {
            if (response.Content.Headers.ContentType?.MediaType != System.Net.Mime.MediaTypeNames.Application.Octet)
                return;

            // Make temp file to recieve to
            var fileOutPath = Path.GetTempPath() + "vsyasgui-download_" + Guid.NewGuid().ToString() + ".txt";
            FileStream? outStream = null;

            try
            {
                outStream = System.IO.File.Create(fileOutPath);
            }
            catch (Exception e)
            {
                outStream?.Close();
                return;
            }

            try
            {
                using (Stream input = response.Content.ReadAsStream())
                {
                    byte[] buffer = new byte[8192];
                    int bytesRead;
                    while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        outStream.Write(buffer, 0, bytesRead);
                    }
                }
            }
            catch
            {
                outStream.Close();
                return;
            }

            outStream.Close();
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
