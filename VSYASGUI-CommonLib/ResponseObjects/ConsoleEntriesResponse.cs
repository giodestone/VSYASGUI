using System.Text.Json.Serialization;

namespace VSYASGUI_CommonLib.ResponseObjects
{
    public class ConsoleEntriesResponse : InstanceAwareResponseBase
    {
        [JsonIgnore]
        public override bool ExpectsResponse => true;

        public List<string>? NewLines { get; set; }

        public long LineFrom { get; set; }

        public long LineTo { get; set; }
    }
}
