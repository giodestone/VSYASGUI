using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSYASGUI_WFP_App.MVVM.Models
{
    /// <summary>
    /// Provides a way of representing if an error has occurred.
    /// </summary>
    internal enum Error
    {
        Ok = 0,
        General,
        Connection,
        Unauthorised

    }
}
