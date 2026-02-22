using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VSYASGUI_CommonLib.RequestObjects
{
    /// <summary>
    /// Base for all objects which request things.
    /// </summary>
    public abstract class RequestBase
    {
        /// <summary>
        /// Where the request should go.
        /// </summary>
        [JsonIgnore]
        protected abstract string Address { get; }

        public string ApiKey = string.Empty;
    }
}
