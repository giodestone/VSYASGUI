using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSYASGUI_CommonLib.RequestObjects
{
    public class CommandRequest : RequestBase
    {
        public override string Address => "/command";

        public string Command { get; set; }

    }
}
