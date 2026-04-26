using System;
using System.Collections.Generic;
using System.Text;

namespace VSYASGUI_CommonLib.ResponseObjects.ClientSide
{
    /// <summary>
    /// Represents a file sent by the server
    /// </summary>
    /// <remarks>
    /// Unlike other responses, this does NOT correpond to an API response. This response should be created on the client-side, as it depends on how files are handled.
    /// </remarks>
    public class FileResponse : ResponseBase
    {
        public override bool ExpectsResponse => true;

        public FileInfo SavedFile;
    }
}
