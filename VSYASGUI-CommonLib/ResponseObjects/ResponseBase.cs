using System.Text.Json.Serialization;

namespace VSYASGUI_CommonLib.ResponseObjects
{
    /// <summary>
    /// Base class for all responses. All classes MUST be serializable.
    /// </summary>
    public abstract class ResponseBase
    {
        /// <summary>
        /// Whether this response expects a body.
        /// </summary>
        [JsonIgnore]
        public abstract bool ExpectsResponse { get; }
    }
}
