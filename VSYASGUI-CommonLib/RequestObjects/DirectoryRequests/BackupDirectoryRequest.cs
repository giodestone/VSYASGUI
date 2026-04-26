using System;
using System.Collections.Generic;
using System.Text;

namespace VSYASGUI_CommonLib.RequestObjects.DirectoryRequests
{
    /// <summary>
    /// Request information about the directory.
    /// </summary>
    public class BackupDirectoryRequest : DirectoryRequest
    {
        public override string Address => "/save-backups";
    }
}
