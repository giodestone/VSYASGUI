using System;
using System.Collections.Generic;
using System.Text;
using VSYASGUI_CommonLib;

namespace VSYASGUI_WFP_App.MVVM.ViewModels.FileRequestProviders
{
    /// <summary>
    /// Provides functions for fetching directory info/downloading file for <see cref="FilePresenter"/>.
    /// </summary>
    internal interface IFileRequestProvider
    {
        public abstract ApiRequest GetFileDownloadRequest(string fileName);
        public abstract ApiRequest GetDirectoryContentsRequest();
    }
}
