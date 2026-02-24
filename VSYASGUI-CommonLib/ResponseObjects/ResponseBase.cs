using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
