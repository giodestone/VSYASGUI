using VSYASGUI_CommonLib.ResponseObjects;

namespace VSYASGUI_CommonLib
{
    public static class ResponseFactory
    {
        public static ErrorResponse MakeErrorUnauthorised() 
        { 
            return new ErrorResponse() { error = "unauthorised" }; 
        }

        public static ErrorResponse MakeErrorBadRequest()
        {
            return new ErrorResponse() { error = "bad-request" };
        }

        public static ConsoleEntriesResponse MakeConsoleEntriesResponse(List<string> lines, long lineFrom, long lineTo, Guid serverInstanceGuid)
        {
            return new ConsoleEntriesResponse() { NewLines = lines, LineFrom = lineFrom, LineTo = lineTo, ServerGuid = serverInstanceGuid };
        }

        public static ConnectionCheckResponse MakeConnectionCheckResponse(Guid serverInstanceGuid)
        {
            return new ConnectionCheckResponse() { ServerGuid = serverInstanceGuid };
        }

        public static ServerStatisticsResponse MakeServerStatisticsResponse(double cpuUsagePercentage, long memoryUsageBytes, int serverUptimeSeconds, int totalWorldPlaytime, int onlinePlayerCount)
        {
            return new ServerStatisticsResponse()
            {
                CpuUsagePercentage = cpuUsagePercentage,
                MemoryUsageBytes = memoryUsageBytes,
                ServerSecondsUptime = serverUptimeSeconds,
                TotalWorldPlaytime = totalWorldPlaytime,
                OnlinePlayerCount = onlinePlayerCount
            };
        }
    }
}
