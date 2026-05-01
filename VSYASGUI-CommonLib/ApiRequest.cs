using System;
using System.Collections.Generic;
using System.Text;

namespace VSYASGUI_CommonLib
{
    /// <summary>
    /// Wrapper for making a RESTful HTTP request.
    /// </summary>
    public sealed class ApiRequest
    {
        /// <summary>
        /// Top level address, the one after the provided API url.
        /// </summary>
        public string ApiEndpointUrl { get; init; }

        /// <summary>
        /// Additional arguments after the endpoint URL.
        /// </summary>
        public List<string> Arguments { get; init; }

        /// <summary>
        /// The method used to send to the server.
        /// </summary>
        public RequestMethods RequestMethod { get; init; }

        /// <summary>
        /// Converts the <see cref="ApiEndpointUrl"/> and <see cref="Arguments"/> into a nice string.
        /// </summary>
        /// <example>
        /// If the API url is <c>http://localhost:8000/</c>, and the <see cref="ApiEndpointUrl"/> is <c>console</c>, the resolved URL will become <c>http://localhost:8000/console</c>.
        /// </example>
        /// <returns></returns>
        /// <param name="endpointUrl">The URL, with a trailing <c>/</c>.</param>
        public string ToAddress(string endpointUrl)
        {
            string concatenatedArgs = string.Empty;
            foreach (var item in Arguments)
            {
                concatenatedArgs += Uri.EscapeDataString(item) + "/";
            }

            return endpointUrl + ApiEndpointUrl + "/" + concatenatedArgs;
        }
    }
}
