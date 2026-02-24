using System.Text.Json.Serialization;

namespace VSYASGUI_CommonLib.ResponseObjects
{
    public class ConsoleEntriesResponse : ResponseBase
    {
        public List<string>? NewLines;

        [JsonIgnore]
        public override bool ExpectsResponse => true;
    }
}
