using System;
using System.Collections.Generic;
using System.Text;
using VSYASGUI_CommonLib;

namespace VSYASGUI_WFP_App.MVVM.ViewModels.FileRequestProviders
{
    /// <summary>
    /// Implements <see cref="RequestFactory"/> methods to provide file interaction functions. See also: <seealso cref="IFileRequestProvider"/>, <seealso cref="FilePresenter"/>.
    /// </summary>
    internal class WorldBackupFileRequestProvider : IFileRequestProvider
    {
        public ApiRequest GetDirectoryContentsRequest()
        {
            return RequestFactory.MakeBackupDirectoryRequest();
        }

        public ApiRequest GetFileDownloadRequest(string fileName)
        {
            return RequestFactory.MakeBackupDownloadRequest(fileName);
        }
    }
}
