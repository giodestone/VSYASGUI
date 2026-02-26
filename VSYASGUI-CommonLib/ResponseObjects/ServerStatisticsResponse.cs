using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VSYASGUI_CommonLib.ResponseObjects
{
    /// <summary>
    /// Responds with the current process resources used by the process.
    /// </summary>
    public class ServerStatisticsResponse : ResponseBase
    {
        [JsonIgnore]
        public override bool ExpectsResponse => true;

        public double CpuUsagePercentage { get; set; }

        public long MemoryUsageBytes { get; set; }

        public int SecondsUptime { get; set; }

        public int TotalWorldPlaytime { get; set; }

        public int OnlinePlayerCount { get; set; }
    }
}
