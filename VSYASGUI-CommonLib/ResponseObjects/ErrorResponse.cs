using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSYASGUI_CommonLib.ResponseObjects
{
    /// <summary>
    /// Signifies an error response.
    /// </summary>
    public sealed class ErrorResponse : ResponseBase
    {
        public string error { get; set; } = "undefined";
    }
}
