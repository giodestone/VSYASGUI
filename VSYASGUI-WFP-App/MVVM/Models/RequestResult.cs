using System.IO;
using System.Runtime.CompilerServices;
using static VSYASGUI_WFP_App.MVVM.Models.ApiConnection;

namespace VSYASGUI_WFP_App.MVVM.Models
{
    internal partial class ApiConnection
    {
        /// <summary>
        /// Content of the request along with the <see cref="Error"/>.
        /// </summary>
        internal struct RequestResult
        {
            public enum MediaType
            {
                Unsupported,
                Json,
                File
            }

            public Error Error { get; private set; }
            public MediaType ResultMediaType { get; private set; }
            public string? HttpContent { get; private set; }
            public FileInfo FileInfo { get; private set; }

            /// <summary>
            /// Request result that returns JSON.
            /// </summary>
            /// <param name="error"></param>
            /// <param name="httpContent"></param>
            /// <returns></returns>
            public static RequestResult FromJson(Error error, string? httpContent)
            {
                return new RequestResult { Error = error, HttpContent = httpContent, ResultMediaType = MediaType.Json };
            }

            /// <summary>
            /// Request result that is saved to a file.
            /// </summary>
            /// <param name="error"></param>
            /// <param name="savedFileInfo"></param>
            /// <returns></returns>
            public static RequestResult FromFile(Error error, FileInfo? savedFileInfo)
            {
                return new RequestResult { Error = error, FileInfo = savedFileInfo, ResultMediaType = MediaType.File };
            }

            /// <summary>
            /// Result which is unsupported.
            /// </summary>
            /// <returns></returns>
            public static RequestResult FromUnsupportedType()
            {
                return new RequestResult { ResultMediaType = MediaType.Unsupported };
            }
        }
    }
}
