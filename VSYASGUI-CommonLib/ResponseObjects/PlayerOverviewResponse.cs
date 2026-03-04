using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSYASGUI_CommonLib.ResponseObjects
{
    public class PlayerOverviewResponse : ResponseBase
    {
        public override bool ExpectsResponse => true;

        public string HashOfPlayerOverviews { get; set; }
        public List<PlayerOverview>? PlayerOverviews { get; set; }
    }
}
