using System.Text.Json.Serialization;

namespace VSYASGUI_CommonLib.RequestObjects
{
    /// <summary>
    /// Base for all objects which request things.
    /// </summary>
    public abstract class RequestBase
    {
        /// <summary>
        /// Where the request should go. Must include a leading /
        /// </summary>
        [JsonIgnore]
        public abstract string Address { get; }

        [JsonIgnore]
        public string ApiKey = string.Empty;
    }
}
