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
        private Task<ApiResponse<ConnectionCheckResponse>> _CurrentConnectionCheckTask;

        public event EventHandler<ConsoleEntriesResponse> ConsoleReadSuccessful;

        private Task<ApiResponse<ConsoleEntriesResponse>> _ConsoleEntryRequestTask;

        public event EventHandler ServerInstanceGuidChanged;
        private Guid _LatestServerGuid = Guid.Empty;

        public event EventHandler<ApiResponse<ConsoleCommandResponse>> SendCommandComplete;
        private Task<ApiResponse<ConsoleCommandResponse>> _SendCommandTask;
        private CancellationTokenSource _SendCommandCancelleationToken;
        private string _SendCommandContents = string.Empty;


        /// <summary>
        /// Command form of <see cref="TryBeginConnectionCheck"/>.
        /// </summary>
        public ICommand TryBeginConnectionCheckCommand => new Command(_ => TryBeginConnectionCheck());

        /// <summary>
        /// Command form of <see cref="TryCancelConnectionCheck"/>.
        /// </summary>
        public ICommand TryCancelConnectionCheckCommand => new Command(_ => TryCancelConnectionCheck());

        /// <summary>
        /// Command form of <see cref="TrySendConsoleCommand(string)"/>
        /// </summary>
        public ICommand TrySendCommandToServerCommand => new Command(_ => TrySendConsoleCommand());

        /// <summary>
        /// The contents of the send command.
        /// </summary>
        public string SendCommandContents
        {
            get => _SendCommandContents;
            set
            {
                _SendCommandContents = value;
                Update(ref _SendCommandContents, value);
            }
        }

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
                _CurrentConnectionCheckTask = ApiConnection.Instance.RequestApiInfo<ConnectionCheckResponse>(new ConnectionRequest(), _ConnectionCheckCancellationTokenSource.Token);
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
        private void OnConnectionCheckComplete(ApiResponse<ConnectionCheckResponse> taskResult)
        {
            CheckInstanceAwareResponseForChange(taskResult);

            ConnectionCheckComplete.Invoke(this, taskResult.ErrorResult);
        }

        /// <summary>
        /// Checks whether a <see cref="InstanceAwareResponseBase"/> has an instance changed, and notifies the subscribers of <see cref="ServerInstanceGuidChanged"/>.
        /// </summary>
        /// <param name="taskResult">Api response to check.</param>
        private void CheckInstanceAwareResponseForChange<T>(ApiResponse<T> taskResult) where T: InstanceAwareResponseBase
        {
            if (taskResult.ErrorResult == Error.Ok && taskResult.Response != null)
            {
                if (_LatestServerGuid == Guid.Empty)
                    _LatestServerGuid = taskResult.Response.ServerGuid;
                else if (taskResult.Response.ServerGuid != _LatestServerGuid)
                {
                    ServerInstanceGuidChanged?.Invoke(this, EventArgs.Empty); // FIXME: I am not happy with where this goes as it may make more sense to have this as a part of ApiConnection? But at the same time, I think it is a reasonable assumption that one endpoint address to be pointing to an equivalent instance, but not neccessairly the same process. Basically when the process changes (e.g. crash) I think some components need to know, but not the whole server. 
                    _LatestServerGuid = taskResult.Response.ServerGuid;
                }
            }
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
            CheckInstanceAwareResponseForChange(response);

            if (response.ErrorResult == Error.Ok && response.Response != null)
                ConsoleReadSuccessful?.Invoke(this, response.Response);
        }


        public bool CanSendConsoleCommand
        {
            get
            {
                if (_ConsoleEntryRequestTask == null)
                    return false;
                if (!_ConsoleEntryRequestTask.IsCompleted)
                    return false;

                return true;
            }
        }

        public bool TrySendConsoleCommand()
        {
            if (!CanSendConsoleCommand)
                return false;

            try
            {
                _SendCommandCancelleationToken = new CancellationTokenSource();
                _SendCommandTask = ApiConnection.Instance.RequestApiInfo<ConsoleCommandResponse>(new CommandRequest() { Command = SendCommandContents }, _SendCommandCancelleationToken.Token);
                _SendCommandTask.ContinueWith(task => Application.Current.Dispatcher.BeginInvoke(OnSendCommandComplete, task.Result));
                SendCommandContents = string.Empty;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: Unable to invoke send console command due to an exception.");
                Console.WriteLine(e.Message);
                OnSendCommandComplete(new ApiResponse<ConsoleCommandResponse>(Error.NotSent, null));
                return false;
            }

            return true;
        }

        private void OnSendCommandComplete(ApiResponse<ConsoleCommandResponse> response)
        {
            SendCommandComplete?.Invoke(this, response);
        }

    }
}
