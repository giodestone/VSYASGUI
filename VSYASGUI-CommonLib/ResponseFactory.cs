using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using VSYASGUI_CommonLib.ResponseObjects;

namespace VSYASGUI_CommonLib
{
    /// <summary>
    /// Factory which creates responses.
    /// </summary>
    public static class ResponseFactory
    {
        /// <summary>
        /// Returns an error signifying an unauthorised error.
        /// </summary>
        public static ErrorResponse MakeErrorUnauthorised() 
        { 
            return new ErrorResponse() { Error = "unauthorised" }; 
        }

        /// <summary>
        /// Returns an error signifing a bad request.
        /// </summary>
        /// <remarks>
        /// The HTTP API may respond with HTTP error codes.
        /// </remarks>
        public static ErrorResponse MakeErrorBadRequest()
        {
            return new ErrorResponse() { Error = "bad-request" };
        }

        /// <summary>
        /// Returns a set-up <see cref="ConsoleEntriesResponse"/>.
        /// </summary>
        /// <param name="lines">The contents of the individual lines.</param>
        /// <param name="lineFrom">The line number of the first line in <paramref name="lines"/>.</param>
        /// <param name="lineTo">The line number of the last line in <paramref name="lines"/>.</param>
        /// <param name="serverInstanceGuid">The GUID of the server. Should be unique for each server launch.</param>
        public static ConsoleEntriesResponse MakeConsoleEntriesResponse(List<string> lines, long lineFrom, long lineTo, Guid serverInstanceGuid)
        {
            return new ConsoleEntriesResponse() { NewLines = lines, LineFrom = lineFrom, LineTo = lineTo, ServerGuid = serverInstanceGuid };
        }

        /// <summary>
        /// Creates a configured <see cref="ConnectionCheckResponse"/>.
        /// </summary>
        /// <param name="serverInstanceGuid">The GUID of the server. Should be unique for each server launch.</param>
        public static ConnectionCheckResponse MakeConnectionCheckResponse(Guid serverInstanceGuid)
        {
            return new ConnectionCheckResponse() { ServerGuid = serverInstanceGuid };
        }

        /// <summary>
        /// Returns a new configures <see cref="ServerStatisticsResponse"/>.
        /// </summary>
        /// <param name="cpuUsagePercentage">The CPU usage as a percentage (0 to 1).</param>
        /// <param name="memoryUsageBytes">The number of bytes the process is using.</param>
        /// <param name="serverUptimeSeconds">The server uptime in seconds.</param>
        /// <param name="totalWorldPlaytime">The total world playtime in seconds.</param>
        /// <param name="onlinePlayerCount">The total number of connected players.</param>
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

        /// <summary>
        /// Returns a configured <see cref="PlayerOverviewResponse"/>.
        /// </summary>
        /// <param name="playerOverviews">Overviews of the players. Can be null if no players are connected.</param>
        /// <returns></returns>
        public static PlayerOverviewResponse MakePlayerOverviewResponse(List<PlayerOverview>? playerOverviews)
        {
            // TODO: Don't like the hashing here, would be better done elsewhere.

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
