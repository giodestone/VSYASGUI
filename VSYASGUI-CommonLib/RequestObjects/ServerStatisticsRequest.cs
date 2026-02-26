using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSYASGUI_CommonLib.RequestObjects
{
    /// <summary>
    /// Request the current server resource usage (CPU, RAM).
    /// </summary>
    public class ServerStatisticsRequest : RequestBase
    {
        public override string Address => "/statistics";
    }
}
