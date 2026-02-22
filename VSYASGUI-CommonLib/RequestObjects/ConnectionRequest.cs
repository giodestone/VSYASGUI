using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSYASGUI_CommonLib.RequestObjects
{
    /// <summary>
    /// For testing the connection.
    /// </summary>
    public class ConnectionRequest : RequestBase
    {
        protected override string Address => "/";
    }
}
