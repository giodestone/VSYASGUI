using System;
using System.Collections.Generic;
using System.Text;

namespace VSYASGUI_CommonLib.ResponseObjects
{
    /// <summary>
    /// Represents a file sent by the server.
    /// </summary>
    public class FileResponse : ResponseBase
    {
        public override bool ExpectsResponse => true;

        public string FileHashSha256;

        public ulong FileLengthBytes;

        public string FileBase64;
    }
}
