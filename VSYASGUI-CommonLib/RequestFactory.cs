using System;
using System.Collections.Generic;
using System.Text;

namespace VSYASGUI_CommonLib
{
    /// <summary>
    /// Generates prepared <see cref="ApiRequest"/>s depending on what is created.
    /// </summary>
    public static class RequestFactory
    {
        /// <summary>
        /// Make a request that is for checking the current request. Expected response: json <see cref="ResponseObjects.ConnectionCheckResponse"/>.
        /// </summary>
        /// <param name="endpointUrl"></param>
        /// <returns></returns>
        public static ApiRequest MakeConnectionCheckRequest()
        {
            return new ApiRequest { ApiEndpointUrl = ApiEndpointConstants.ConnectionCheckAddress, RequestMethod=RequestMethods.GET };
        }

        /// <summary>
        /// Make a request that gives the player overview. Expectd response: json <see cref="ResponseObjects.PlayerOverviewResponse"/>
        /// </summary>
        public static ApiRequest MakePlayerOverviewRequest()
        {
            return new ApiRequest { ApiEndpointUrl = ApiEndpointConstants.PlayerOverviewAddress, RequestMethod=RequestMethods.GET };
        }

        /// <summary>
        /// Request all console entries from a certain line. Expected response: json <see cref="ResponseObjects.ConsoleEntriesResponse"/>
        /// </summary>
        /// <remarks>
        /// May respond with the latest line that was cached from.
        /// </remarks>
        /// <param name="lineFrom">The line from which to request</param>
        public static ApiRequest MakeConsoleLineSinceRequest(long lineFrom)
        {
            return new ApiRequest { ApiEndpointUrl = ApiEndpointConstants.ConsoleFromAddress, RequestMethod=RequestMethods.GET, Arguments = { lineFrom.ToString() } };
        }

        /// <summary>
        /// Injects a command into the console command. Expected response: json <see cref="ResponseObjects.ConsoleCommandResponse"/>.
        /// </summary>
        /// <param name="commandToInjectIntoConsole"></param>
        /// <returns></returns>
        public static ApiRequest MakeConsoleCommandRequest(string commandToInjectIntoConsole)
        {
            return new ApiRequest { ApiEndpointUrl = ApiEndpointConstants.ConsolePostAddress, RequestMethod = RequestMethods.POST, Arguments = { commandToInjectIntoConsole } };
        }

        /// <summary>
        /// Get the contents of the backup directory. Expected response: json <see cref="ResponseObjects.DirectoryResponse"/>.
        /// </summary>
        public static ApiRequest MakeBackupDirectoryRequest()
        {
            return new ApiRequest { ApiEndpointUrl = ApiEndpointConstants.BackupDirectoryAddress, RequestMethod = RequestMethods.GET };
        }

        /// <summary>
        /// Get a certain file from the backup directory. Expected response octlet, and as a result depends on the client implementation as it begins a download stream!
        /// </summary>
        /// <param name="fileName">The name of the file in the requested file in the directory.</param>
        public static ApiRequest MakeBackupDownloadRequest(string fileName)
        {
            return new ApiRequest { ApiEndpointUrl = ApiEndpointConstants.BackupDownloadAddress, RequestMethod = RequestMethods.GET, Arguments = { fileName } };
        }

        /// <summary>
        /// Get the current server statistics. Expected response: json <see cref="ResponseObjects.ServerStatisticsResponse"/>.
        /// </summary>
        public static ApiRequest MakeStatisticsRequest()
        {
            return new ApiRequest { ApiEndpointUrl = ApiEndpointConstants.ServerStatisticsAddress, RequestMethod = RequestMethods.GET };
        }
    }
}
