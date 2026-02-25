using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSYASGUI_CommonLib.ResponseObjects
{
    /// <summary>
    /// For responses that need to take into account the GUID of the sender.
    /// </summary>
    public abstract class InstanceAwareResponseBase : ResponseBase
    {
        public Guid ServerGuid { get; set; }
    }
}
