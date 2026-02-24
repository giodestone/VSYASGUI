using System.Text.Json.Serialization;

namespace VSYASGUI_CommonLib.RequestObjects
{
    public class ConsoleRequest : RequestBase
    {
        [JsonIgnore]
        public override string Address => "/console";

        public long LineFrom { get; set; } = 0;
    }
}
