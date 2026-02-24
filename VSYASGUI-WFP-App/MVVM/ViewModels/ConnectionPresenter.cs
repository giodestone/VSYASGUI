using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VSYASGUI_CommonLib.RequestObjects;
using VSYASGUI_CommonLib.ResponseObjects;
using VSYASGUI_WFP_App.MVVM.Models;
using VSYASGUI_WFP_App.MVVM.ViewModels.Base;

namespace VSYASGUI_WFP_App.MVVM.ViewModels
{
    internal sealed class ConnectionPresenter : Presenter
    {
        public event EventHandler ConnectionCheckBegun;
        public event EventHandler<Error> ConnectionCheckComplete;
        
        private CancellationTokenSource _ConnectionCheckCancellationTokenSource;
        private Task<ApiResponse<NoResponse>> _CurrentConnectionCheckTask;

        public event EventHandler<ConsoleEntriesResponse> ConsoleReadSuccessful;

        private Task<ApiResponse<ConsoleEntriesResponse>> _ConsoleEntryRequestTask;

        /// <summary>
        /// Command form of <see cref="TryBeginConnectionCheck"/>.
        /// </summary>
        public ICommand TryBeginConnectionCheckCommand => new Command(_ => TryBeginConnectionCheck());

        /// <summary>
        /// Command form of <see cref="TryCancelConnectionCheck"/>.
        /// </summary>
        public ICommand TryCancelConnectionCheckCommand => new Command(_ => TryCancelConnectionCheck());

        /// <summary>
        /// Calls <see cref="ApiConnection.SetupConnection(string, string)"/> if <see cref="ApiConnection.Instance"/> is already null.
        /// </summary>
        public ConnectionPresenter()
        {
            if (ApiConnection.Instance == null)
                ApiConnection.SetupConnection(Config.Instance.GetUrlForApi, Config.Instance.CurrentApiKey);
        }

        private bool IsConnectionTaskRunning(Task task)
        { 
            return ApiConnection.Instance != null && (task != null && !task.IsCompleted); 
        }

        /// <summary>
        /// Try to check if it is possible to connect with the server.
        /// </summary>
        /// <returns><c>true</c> if check was begun, <c>false</c> if the check is already running, or in rare cases cannot start.</returns>
        public bool TryBeginConnectionCheck()
        {
            if (IsConnectionTaskRunning(_CurrentConnectionCheckTask))
                return false;

            ConnectionCheckBegun?.Invoke(this, EventArgs.Empty);
            _ConnectionCheckCancellationTokenSource = new CancellationTokenSource();
            try
            {
                _CurrentConnectionCheckTask = ApiConnection.Instance.RequestApiInfo<NoResponse>(new ConnectionRequest(), _ConnectionCheckCancellationTokenSource.Token);
                _CurrentConnectionCheckTask.ContinueWith(task => Application.Current.Dispatcher.BeginInvoke(OnConnectionCheckComplete, task.Result));
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: Unable to invoke connection check due to an exception.");
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Cancel the current connection check task (if possible).
        /// </summary>
        public void TryCancelConnectionCheck()
        {
            try
            {
                _ConnectionCheckCancellationTokenSource.Cancel();
            }
            catch (Exception ex)
            {
                Console.WriteLine("WARNING: Unable to cancel the task due to the following exception.");
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Callback for when <see cref="_CurrentConnectionCheckTask"/> completes.
        /// </summary>
        /// <remarks>
        /// Must be called by the main thread, e.g. using <c>Application.Current.Dispatcher.BeginInvoke(OnConnectionCheckComplete>)</c>.
        /// </remarks>
        /// <param name="taskResult">Result of the task.</param>
        private void OnConnectionCheckComplete(ApiResponse<NoResponse> taskResult)
        {
            ConnectionCheckComplete.Invoke(this, taskResult.ErrorResult);
        }

        public bool TryRequestConsoleUpdate(long lineFrom)
        {
            if (IsConnectionTaskRunning(_ConsoleEntryRequestTask))
                return false;

            try
            {
                _ConsoleEntryRequestTask = ApiConnection.Instance.RequestApiInfo<ConsoleEntriesResponse>(new ConsoleRequest() { LineFrom = lineFrom }, new CancellationToken());
                _ConsoleEntryRequestTask.ContinueWith(task => Application.Current.Dispatcher.BeginInvoke(OnRequestConsoleUpdateSucceeded, task.Result));
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: Unable to invoke connection check due to an exception.");
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        private void OnRequestConsoleUpdateSucceeded(ApiResponse<ConsoleEntriesResponse> response)
        {
            if (response.ErrorResult == Error.Ok && response.Response != null)
                ConsoleReadSuccessful?.Invoke(this, response.Response);
        }
    }
}
