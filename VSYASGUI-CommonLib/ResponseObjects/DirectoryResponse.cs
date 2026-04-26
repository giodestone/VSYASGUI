using System;
using System.Collections.Generic;
using System.Text;

namespace VSYASGUI_CommonLib.ResponseObjects
{
    /// <summary>
    /// Represents a directory listicle.
    /// </summary>
    public class DirectoryResponse : ResponseBase
    {
        public override bool ExpectsResponse => true;

        public List<string> Files;
    }
}
