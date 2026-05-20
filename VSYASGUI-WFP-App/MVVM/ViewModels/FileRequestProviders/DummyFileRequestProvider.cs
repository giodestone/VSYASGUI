using System;
using System.Collections.Generic;
using System.Text;
using VSYASGUI_CommonLib;

namespace VSYASGUI_WFP_App.MVVM.ViewModels.FileRequestProviders
{
    /// <summary>
    /// For providing sample values, e.g. at default constructor. See also: <seealso cref="IFileRequestProvider"/>, <seealso cref="FilePresenter"/>.
    /// </summary>
    internal class DummyFileRequestProvider : IFileRequestProvider
    {
        public ApiRequest GetDirectoryContentsRequest()
        {
            return RequestFactory.MakeConnectionCheckRequest();
        }

        public ApiRequest GetFileDownloadRequest(string fileName)
        {
            return RequestFactory.MakeConnectionCheckRequest();
        }
    }
}
