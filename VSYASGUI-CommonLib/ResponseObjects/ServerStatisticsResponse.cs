using System.Text.Json.Serialization;

namespace VSYASGUI_CommonLib.ResponseObjects
{
    /// <summary>
    /// Responds with overview statistics of the running server.
    /// </summary>
    public class ServerStatisticsResponse : ResponseBase
    {
        [JsonIgnore]
        public override bool ExpectsResponse => true;

        public double CpuUsagePercentage { get; set; }

        public long MemoryUsageBytes { get; set; }

        public int ServerSecondsUptime { get; set; }

        public int TotalWorldPlaytime { get; set; }

        public int OnlinePlayerCount { get; set; }
    }
}
