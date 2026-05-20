using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Input;
using VSYASGUI_CommonLib;
using VSYASGUI_CommonLib.FileManagement;
using VSYASGUI_CommonLib.ResponseObjects;
using VSYASGUI_CommonLib.ResponseObjects.ClientSide;
using VSYASGUI_WFP_App.MVVM.Models;
using VSYASGUI_WFP_App.MVVM.ViewModels.Base;
using VSYASGUI_WFP_App.MVVM.ViewModels.FileRequestProviders;

namespace VSYASGUI_WFP_App.MVVM.ViewModels
{
    /// <summary>
    /// Provides file operations such as viewing contents and downloading a file, along with progress reporting.
    /// 
    /// Customisable in order to provide more than one possible implementation
    /// </summary>
    internal class FilePresenter : Presenter
    {
        private readonly IFileRequestProvider _FileRequestProvider;

        private Task<ApiResponse<DirectoryResponse>>? _DirectoryLookupTask;
        private CancellationTokenSource? _DirectoryCancellationTokenSource;
        private ObservableCollection<ApiFileInfo> _DirectoryFiles = new();
        private int _FileSelectedIndex = -1;
        private DateTime _LastSuccessfulRefresh = DateTime.UnixEpoch;
        private bool _IsDownloadInProgress = false;

        private Task<ApiResponse<FileResponse>>? _DownloadFileRequestTask;
        private CancellationTokenSource? _DownloadFileCancellationTokenSource;

        private double _FileDownloadProgressValue = 0.0;
        private Progress<double>? _FileDownloadProgress;

        /// <summary>
        /// Contents of the directory and its files.
        /// </summary>
        public ObservableCollection<ApiFileInfo> DirectoryFiles
        {
            get { return _DirectoryFiles; }
            set 
            { 
                UpdateFieldWithValue(ref _DirectoryFiles, value, nameof(DirectoryFiles));
                NotifyFieldUpdated(nameof(CanDownloadFile));
            }
        }

        /// <summary>
        /// Which file is currently selected.
        /// </summary>
        public int SelectedFileIndex
        {
            get => _FileSelectedIndex;
            set
            {
                UpdateFieldWithValue(ref _FileSelectedIndex, value, nameof(DirectoryFiles));
                NotifyFieldUpdated(nameof(CanDownloadFile));
            }
        }

        /// <summary>
        /// When <see cref="OnDirectoryContentsRequestComplete(ApiResponse{DirectoryResponse})"/> last completed successfully.
        /// </summary>
        public DateTime LastSuccessfulDirectoryRefresh
        {
            get => _LastSuccessfulRefresh;
            set
            {
                UpdateFieldWithValue(ref _LastSuccessfulRefresh, value, nameof(LastSuccessfulDirectoryRefresh));
                NotifyFieldUpdated(nameof(LastSuccessfulDirectoryRefreshHumanReadable));
            }
        }

        /// <summary>
        /// Returns a more human readable version of <see cref="LastSuccessfulDirectoryRefresh"/>, where if never updated, it tells user it has never been updated.
        /// </summary>
        public string LastSuccessfulDirectoryRefreshHumanReadable
        {
            get
            {
                if (LastSuccessfulDirectoryRefresh == DateTime.UnixEpoch)
                    return "Never";
                else
                    return LastSuccessfulDirectoryRefresh.ToString();
            }
        }

        /// <summary>
        /// Whether there is currently a file download in progress.
        /// </summary>
        public bool IsFileDownloadInProgress
        {
            get => _IsDownloadInProgress;
            set
            {
                UpdateFieldWithValue(ref _IsDownloadInProgress, value, nameof(IsFileDownloadInProgress));
                NotifyFieldUpdated(nameof(CanDownloadFile));
            }
        }

        /// <summary>
        /// Whether it is possible to download a file. Combo of <see cref="IsFileDownloadInProgress"/>, and bound checking of <see cref="SelectedFileIndex"/> with <see cref="DirectoryFiles"/>.
        /// </summary>
        public bool CanDownloadFile
        {
            get
            {
                if (IsFileDownloadInProgress)
                    return false;

                if (SelectedFileIndex < 0 && SelectedFileIndex >= DirectoryFiles.Count && DirectoryFiles.Count > 0)
                    return false;

                return true;
            }
        }

        /// <summary>
        /// The current value of the download. See also: <seealso cref="MinDownloadProgressValue"/>, <seealso cref="MaxDownloadProgressValue"/>.
        /// </summary>
        public double DownloadProgress
        {
            get
            {
                return _FileDownloadProgressValue;
            }

            set => UpdateFieldWithValue(ref _FileDownloadProgressValue, value, nameof(DownloadProgress));
        }

        /// <summary>
        /// Minimum possible value of <see cref="DownloadProgress"/>.
        /// </summary>
        public double MinDownloadProgressValue => 0.0;

        /// <summary>
        /// Maximum possible value of <see cref="DownloadProgress"/>.
        /// </summary>
        public double MaxDownloadProgressValue => 1.0;

        /// <summary>
        /// Try to request the saved world.
        /// </summary>
        public ICommand TryRequestFileSaveCommand => new Command(_ => TryRequestFileDownload());

        /// <summary>
        /// Try to request the contents of the backup directory.
        /// </summary>
        public ICommand TryRequestDirectoryContentsCommand => new Command(_ => TryRequestDirectoryContents());


        /// <summary>
        /// Create with a dummy file request provider. Will not provide full functionality if called.
        /// </summary>
        public FilePresenter()
        {
            _FileRequestProvider = new DummyFileRequestProvider();
        }

        /// <summary>
        /// Create with a file request provider.
        /// </summary>
        /// <param name="fileRequestProvider"></param>
        public FilePresenter(IFileRequestProvider fileRequestProvider)
        {
            _FileRequestProvider = fileRequestProvider;
        }

        /// <summary>
        /// Try to request a file download and set up download progress, if not already setup.
        /// </summary>
        /// <returns><c>true</c> if possible; <c>false</c> if not.</returns>
        public bool TryRequestFileDownload()
        {
            // TODO: this is a bit of a mess, refactoring this would require expanding below to just check. However, then the task running would need to become an updatable property which would involve figuring out state tracking, which IsFileDownloading kind of wraps around.
            if (!CanDownloadFile)
                return false;

            if (TaskHelpers.IsTaskRunning(_DownloadFileRequestTask))
                return false;

            if (IsFileDownloadInProgress)
                return false;

            if (_FileDownloadProgress == null)
            {
                _FileDownloadProgress = new();
                _FileDownloadProgress.ProgressChanged += OnDownloadProgressUpdated;
            }

            try
            {
                IsFileDownloadInProgress = true;
                _DownloadFileCancellationTokenSource = new CancellationTokenSource();
                _DownloadFileRequestTask = ApiConnection.Instance.RequestFileFromApi(_FileRequestProvider.GetFileDownloadRequest(DirectoryFiles[SelectedFileIndex].FileName), _DownloadFileCancellationTokenSource.Token, _FileDownloadProgress);
                _DownloadFileRequestTask.ContinueWith(task => Application.Current.Dispatcher.BeginInvoke(OnFileDownloadComplete, task.Result));
            }
            catch (Exception e)
            {
                IsFileDownloadInProgress = false;
                Console.WriteLine("ERROR: Unable to invoke task due to an exception.");
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Attempts to request the contents of the directory.
        /// </summary>
        /// <returns><c>true</c> if requested and possible, <c>false</c> if not.</returns>
        public bool TryRequestDirectoryContents()
        {
            return TaskHelpers.TryMakeRequest<DirectoryResponse>(ref _DirectoryLookupTask, _DirectoryCancellationTokenSource, _FileRequestProvider.GetDirectoryContentsRequest(), OnDirectoryContentsRequestComplete) == Error.Ok;
        }
        
        /// <summary>
        /// Callback for when the file has been successfully downloaded to disk.
        /// </summary>
        /// <param name="response">The complete response.</param>
        private void OnFileDownloadComplete(ApiResponse<FileResponse> response)
        {
            if (!IsFileDownloadInProgress)
            {
                Console.Error.WriteLine(nameof(ConnectionPresenter) + ": the variable " + nameof(IsFileDownloadInProgress) + " should be true at this point! The download will continue, but this may cause disaster at some point, and is a programming error.");
            }

            if (response.ErrorResult != Error.Ok)
            {
                MessageBox.Show("Failed to download the save download. \n\nError: " + response.ErrorResult, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                IsFileDownloadInProgress = false;
                return;
            }

            if (response.Response == null)
            {
                MessageBox.Show("Failed to download save due to programming error. Please submit a bug report on the GitHub issue page.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                IsFileDownloadInProgress = false;
                return;
            }

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "WorldBackup_" + DateTime.Now.ToString("HH_mm_ss_dd_MM_yyyy"); // Default file name
            dlg.DefaultExt = ".vcdbs"; // Default file extension
            dlg.Filter = "Vintage Story World Files (*.vcdbs) |*.vcdbs|All Files(*.*)|*.*";

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;

                response.Response.SavedFile.MoveTo(filename, true);
            }


            _FileDownloadProgress = null;
            DownloadProgress = 0.0;
            IsFileDownloadInProgress = false;
        }

        /// <summary>
        /// For when the requested directory contents have been requested.
        /// </summary>
        /// <param name="response">The provided response by the server.</param>
        private void OnDirectoryContentsRequestComplete(ApiResponse<DirectoryResponse> response)
        {
            // TODO: Make this nicer maybe? It clears the whole field, which isn't good.

            if (response.ErrorResult != Error.Ok)
                return;

            if (response.Response == null)
            {
                Console.Error.WriteLine(nameof(ConnectionPresenter) + ": Request has no response, when one was expected.");
                return;
            }

            LastSuccessfulDirectoryRefresh = DateTime.Now;

            DirectoryFiles = new ObservableCollection<ApiFileInfo>(response.Response.FileInfos);
        }

        /// <summary>
        /// Callback for when the file download progress changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDownloadProgressUpdated(object? sender, double e)
        {
            DownloadProgress = e;
        }
    }
}
