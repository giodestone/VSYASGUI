using System;
using System.Collections.Generic;
using System.Text;

namespace VSYASGUI_CommonLib
{
    /// <summary>
    /// HTTP request methods, but in an enum.
    /// </summary>
    public enum RequestMethods
    {
        Undefined,
        GET,
        POST,
        PUT,
        DELETE,
        PATCH
    }
}
