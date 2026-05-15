using System;
using System.Collections.Generic;
using System.Text;

namespace VSYASGUI_CommonLib.FileManagement
{
    /// <summary>
    /// Represents select parameters from a <see cref="FileInfo"/> given by the API when processing a directory information request.
    /// </summary>
    /// <remarks>
    /// Should be serialisable.
    /// </remarks>
    public struct ApiFileInfo
    {
        public string FileName { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastWriteTime { get; set; }
        public long Length { get; set; }

        public double FileLengthMb
        {
            get => Math.Ceiling((Length / 1024.0) / 1024.0);
        }

        /// <summary>
        /// Create an ApiFileInfo from a <see cref="FileInfo"/>.
        /// </summary>
        /// <param name="fileInfo">File info to take information from.</param>
        public static ApiFileInfo FromFileInfo(FileInfo fileInfo)
        {
            return new ApiFileInfo { FileName = fileInfo.Name, CreationTime = fileInfo.CreationTime, LastWriteTime = fileInfo.LastWriteTime, Length = fileInfo.Length };
        }
    }
}
