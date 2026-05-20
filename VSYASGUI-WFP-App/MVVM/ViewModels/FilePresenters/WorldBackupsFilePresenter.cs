using System;
using System.Collections.Generic;
using System.Text;
using VSYASGUI_CommonLib;

namespace VSYASGUI_WFP_App.MVVM.ViewModels.FilePresenters
{
    /// <summary>
    /// Provides an interface to download a specific file.
    /// </summary>
    internal sealed class WorldBackupsFilePresenter : FilePresenter
    {
        protected override ApiRequest FileRequestFactoryMethod(string selectedFileName)
        {
            return RequestFactory.MakeBackupDownloadRequest(selectedFileName);
        }

        protected override ApiRequest DirectoryContentsRequest()
        {
            return RequestFactory.MakeBackupDirectoryRequest();
        }
    }
}
