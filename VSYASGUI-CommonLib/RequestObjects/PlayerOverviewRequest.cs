using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSYASGUI_CommonLib.RequestObjects
{
    /// <summary>
    /// Request overview of all players.
    /// </summary>
    public class PlayerOverviewRequest : RequestBase
    {
        public override string Address => "/players";
    }
}
