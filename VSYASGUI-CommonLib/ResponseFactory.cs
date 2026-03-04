using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Unicode;
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

        public static PlayerOverviewResponse MakePlayerOverviewResponse(List<PlayerOverview>? playerOverviews)
        {
            // TODO: Don't like this here, would be better done elsewhere.

            string playerOverviewHashString = Random.Shared.Next().ToString();

            try
            {
                var playerOverviewsJsonBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(playerOverviews));
                var playerOverviewHashBytes = SHA256.Create().ComputeHash(playerOverviewsJsonBytes);
                playerOverviewHashString = Encoding.UTF8.GetString(playerOverviewHashBytes);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Failed to make hash of playerOverviews for PlayerOverviewResponse. Selecting randon hash.");
                Console.Error.WriteLine(e.Message);
            }

            return new PlayerOverviewResponse()
            {
                PlayerOverviews = playerOverviews,
                HashOfPlayerOverviews = playerOverviewHashString
            };
        }
    }
}
