using System;
using System.Collections.Generic;
using System.Text;

namespace VSYASGUI_CommonLib
{
    /// <summary>
    /// Constants related to the API.
    /// </summary>
    public static class ApiEndpointConstants
    {
        public const string ConnectionCheckAddress = ""; // top level, nothing technically as / is assumed to be provided in the overall address.

        public const string ServerStatisticsAddress = "statistics";

        public const string PlayerOverviewAddress = "player-overviews";
        
        public const string ConsoleFromAddress = "console-from";
        public const string ConsolePostAddress = "console";

        public const string BackupDirectoryAddress = "backups";
        public const string BackupDownloadAddress = "backups";
    }
}
