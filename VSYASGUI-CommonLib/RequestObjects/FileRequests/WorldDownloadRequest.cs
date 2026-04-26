using System;
using System.Collections.Generic;
using System.Text;

namespace VSYASGUI_CommonLib.RequestObjects.FileRequests
{
    /// <summary>
    /// Request a download of the current world. Responds with <see cref="ResponseObjects.ClientSide.FileResponse"/>
    /// </summary>
    public class WorldDownloadRequest : FileRequest
    {
        public override string Address => "/backup-download";

        public string FileName;
    }
}
