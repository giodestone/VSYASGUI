using System;
using System.Collections.Generic;
using System.Text;
using VSYASGUI_CommonLib.FileManagement;

namespace VSYASGUI_CommonLib.ResponseObjects
{
    /// <summary>
    /// Represents a directory listicle.
    /// </summary>
    public class DirectoryResponse : ResponseBase
    {
        public override bool ExpectsResponse => true;

        public List<ApiFileInfo> FileInfos;
    }
}
