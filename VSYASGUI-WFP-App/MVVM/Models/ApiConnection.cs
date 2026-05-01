using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows.Media.Animation;
using VSYASGUI_CommonLib;
using VSYASGUI_CommonLib.ResponseObjects;
using VSYASGUI_CommonLib.ResponseObjects.ClientSide;
using static System.Net.WebRequestMethods;

namespace VSYASGUI_WFP_App.MVVM.Models
{
    /// <summary>
    /// Represents a connection to the VS Server.
    /// </summary>
    internal partial class ApiConnection
    {
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
        public async Task<ApiResponse<TExpectedResponse>> RequestApiInfoJson<TExpectedResponse>(ApiRequest request, CancellationToken cancellationToken) where TExpectedResponse : ResponseBase, new()
        {
            var response = await SendHttpRequest(request, cancellationToken);

            if (response.Error != Error.Ok)
                return new ApiResponse<TExpectedResponse>(response.Error, null);

            if (response.ResultMediaType != RequestResult.MediaType.Json)
                return new ApiResponse<TExpectedResponse>(Error.UnexpectedResponse, null);

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
        /// Request a file from a server.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>A task which can be cancelled.</returns>
        public async Task<ApiResponse<FileResponse>> RequestFileFromApi(ApiRequest request, CancellationToken cancellationToken)
        {
            var response = await SendHttpRequest(request, cancellationToken, true);

            if (response.ResultMediaType != RequestResult.MediaType.File)
                return new ApiResponse<FileResponse>(Error.UnexpectedResponse, null);

            return new ApiResponse<FileResponse>(response.Error, new FileResponse() { SavedFile = response.FileInfo });
        }

        /// <summary>
        /// Send a request to the server.
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A cancellable task with the result of the response. The response contains information and the content, if applicable.</returns>
        private async Task<RequestResult> SendHttpRequest(ApiRequest request, CancellationToken cancellationToken, bool tolerateNonJsonResponses=false)
        {
            HttpResponseMessage? response = null;

            // SEND
            try
            {
                // TODO: fix this to include the api key in the header
                HttpContent c = SerialiseObject(request);
                c.Headers.Add(CommonVariables.RequestHeaderApiKeyName, Config.Instance.CurrentApiKey);

                response = await MakeRequestAccordingToRequestMethod(request, cancellationToken);

                response?.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException httpRequestException)
            {
                if (httpRequestException.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return RequestResult.FromJson(Error.Unauthorised, null);
                else if (httpRequestException.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    return RequestResult.FromJson(Error.BadRequest, null);
                return RequestResult.FromJson(Error.General, null);
            }
            catch (TaskCanceledException cancelledException)
            {
                return RequestResult.FromJson(Error.Cancelled, null);
            }
            catch (Exception e)
            {
                return RequestResult.FromJson(Error.General, null);
            }

            if (response == null)
            {
                return RequestResult.FromJson(Error.Connection, null);
            }

            // RESPOND
            if (response.Content.Headers.ContentType?.MediaType == System.Net.Mime.MediaTypeNames.Application.Octet && tolerateNonJsonResponses)
            {
                // this is a file to be downloaded - must be handled appropriately
                return await HandleFileResponse(cancellationToken, response);
            }
            else if (response.Content.Headers.ContentType?.MediaType == System.Net.Mime.MediaTypeNames.Application.Json)
            {
                return await HandleJsonResponse(response);
            }
            else
            {
                return RequestResult.FromUnsupportedType();
            }
        }

        /// <summary>
        /// Call the relevant function in the client to request the file.
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Throws exceptions on sending the messages, or from <see cref="ApiRequest.ToAddress(string)"/>.</exception>
        private async Task<HttpResponseMessage?> MakeRequestAccordingToRequestMethod(ApiRequest request, CancellationToken cancellationToken)
        {
            HttpResponseMessage? response = null;

            switch (request.RequestMethod)
            {
                case RequestMethods.DELETE:
                    response = await _Client.DeleteAsync(request.ToAddress(_EndpointUri), cancellationToken);
                    break;
                case RequestMethods.GET:
                    response = await _Client.GetAsync(request.ToAddress(_EndpointUri), cancellationToken);
                    break;
                case RequestMethods.PATCH:
                    response = await _Client.PatchAsync(request.ToAddress(_EndpointUri), null, cancellationToken);
                    break;
                case RequestMethods.POST:
                    response = await _Client.PostAsync(request.ToAddress(_EndpointUri), null, cancellationToken);
                    break;
                case RequestMethods.PUT:
                    response = await _Client.PostAsync(request.ToAddress(_EndpointUri), null, cancellationToken);
                    break;
                case RequestMethods.Undefined:
                    throw new Exception("Invalid request method.");
            }

            return response;
        }

        /// <summary>
        /// Handles a JSON response.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private static async Task<RequestResult> HandleJsonResponse(HttpResponseMessage response)
        {
            // This can be parsed into a response type.
            string contentString = await response.Content.ReadAsStringAsync();

            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    return RequestResult.FromJson(Error.Ok, contentString);
                case System.Net.HttpStatusCode.Unauthorized:
                    return RequestResult.FromJson(Error.Unauthorised, contentString);
                default:
                    return RequestResult.FromJson(Error.General, contentString);
            }
        }

        /// <summary>
        /// Handles a file response.
        /// </summary>
        /// <param name="response"></param>
        /// <exception cref="NotImplementedException"></exception>
        /// <returns>Task with completion success.</returns>
        private async Task<RequestResult> HandleFileResponse(CancellationToken cancellationToken, HttpResponseMessage response)
        {
            if (response.Content.Headers.ContentType?.MediaType != System.Net.Mime.MediaTypeNames.Application.Octet)
                return RequestResult.FromFile(Error.BadType, null);

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
                return RequestResult.FromFile(Error.FileError, null);
            }

            FileInfo outFileInfo = new FileInfo(fileOutPath);

            try
            {
                using (Stream input = response.Content.ReadAsStream())
                {
                    byte[] buffer = new byte[8192];
                    int bytesRead;
                    while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            outStream.Close();
                            outFileInfo.Delete();
                            return RequestResult.FromFile(Error.Cancelled, null);
                        }

                        outStream.Write(buffer, 0, bytesRead);
                    }
                }
            }
            catch
            {
                outStream.Close();

                try
                {
                    outFileInfo.Delete();
                }
                catch { }

                return RequestResult.FromFile(Error.StreamError, null);
            }

            outStream.Close();
            return RequestResult.FromFile(Error.Ok, outFileInfo);
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
