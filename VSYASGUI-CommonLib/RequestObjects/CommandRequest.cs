using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSYASGUI_CommonLib.RequestObjects
{
    /// <summary>
    /// Text command which should be run by the server. Related: <seealso cref="VSYASGUI_CommonLib.ResponseObjects.ConsoleCommandResponse"/>
    /// </summary>
    public class CommandRequest : RequestBase
    {
        public override string Address => "/command";

        public string Command { get; set; }

    }
}
