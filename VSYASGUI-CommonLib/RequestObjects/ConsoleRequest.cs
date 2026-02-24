using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSYASGUI_CommonLib.RequestObjects
{
    public class ConsoleRequest : RequestBase
    {
        protected override string Address => "/console";
    }
}
