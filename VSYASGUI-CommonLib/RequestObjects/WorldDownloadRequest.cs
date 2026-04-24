using System;
using System.Collections.Generic;
using System.Text;

namespace VSYASGUI_CommonLib.RequestObjects
{
    /// <summary>
    /// Request a download of the current world. Responds with <see cref="ResponseObjects.FileResponse"/>
    /// </summary>
    public class WorldDownloadRequest : RequestBase
    {
        public override string Address => "/world-download";
    }
}
