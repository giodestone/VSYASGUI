using System.Text.Json.Serialization;

namespace VSYASGUI_CommonLib.ResponseObjects
{
    /// <summary>
    /// Returns the provided new lines (if any).
    /// 
    /// Response for <see cref="VSYASGUI_CommonLib.RequestObjects.ConsoleRequest"/>.
    /// </summary>
    public class ConsoleEntriesResponse : InstanceAwareResponseBase
    {
        [JsonIgnore]
        public override bool ExpectsResponse => true;

        public List<string>? NewLines { get; set; }

        public long LineFrom { get; set; }

        public long LineTo { get; set; }
    }
}
