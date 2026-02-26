using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

        private Task _PollServerTask;
        private CancellationTokenSource _PollServerCancellationTokenSource;
        private long _ConsoleContentsLatestLogLine = 0;
        private string _ConsoleContents = string.Empty;



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
            set => UpdateFieldWithValue(ref _SendCommandContents, value, "SendCommandContents");   
        }

        /// <summary>
        /// Get the current contents of the server console that the client knows about.
        /// <br/>
        /// See also: <seealso cref="OnRequestConsoleUpdateSucceeded(ApiResponse{ConsoleEntriesResponse})"/>.
        /// </summary>
        public string ConsoleContents
        {
            get => _ConsoleContents;
            set => UpdateFieldWithValue(ref _ConsoleContents, value, nameof(ConsoleContents));
        }

        /// <summary>
        /// Whether a console command can be sent (i.e. one is not currently being sent).
        /// </summary>
        public bool CanSendConsoleCommand()
        {
            if (_ConsoleEntryRequestTask == null)
                return false;
            if (!_ConsoleEntryRequestTask.IsCompleted)
                return false;

            return true;
        }

        /// <summary>
        /// Calls <see cref="ApiConnection.SetupConnection(string, string)"/> if <see cref="ApiConnection.Instance"/> is already null.
        /// </summary>
        public ConnectionPresenter()
        {
            if (ApiConnection.Instance == null)
                ApiConnection.SetupConnection(Config.Instance.GetUrlForApi, Config.Instance.CurrentApiKey);

            _PollServerCancellationTokenSource = new CancellationTokenSource();
            _PollServerTask = PollServer(_PollServerCancellationTokenSource.Token, OnPollServerInterval);
            ServerInstanceGuidChanged += OnServerGuidChanged_UpdateConsoleLog;
        }

        /// <summary>
        /// Try to check if it is possible to connect with the server.
        /// </summary>
        /// <returns><c>true</c> if check was begun, <c>false</c> if the check is already running, or in rare cases cannot start.</returns>
        public bool TryBeginConnectionCheck()
        {
            if (IsTaskRunning(_CurrentConnectionCheckTask))
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
        /// Try to send the command set in <see cref="SendCommandContents"/> to the server.
        /// </summary>
        /// <returns><c>true</c> if a command has been sent. <c>false</c> if not, due to the value of <see cref="CanSendConsoleCommand"/> or technical error.</returns>
        public bool TrySendConsoleCommand()
        {
            if (!CanSendConsoleCommand())
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

        /// <summary>
        /// Try to get the current value of the server console.
        /// </summary>
        /// <returns>true if requested, false if not</returns>
        private bool TryRequestConsoleUpdate()
        {
            if (IsTaskRunning(_ConsoleEntryRequestTask))
                return false;

            try
            {
                _ConsoleEntryRequestTask = ApiConnection.Instance.RequestApiInfo<ConsoleEntriesResponse>(new ConsoleRequest() { LineFrom = _ConsoleContentsLatestLogLine }, _PollServerCancellationTokenSource.Token);
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

        /// <summary>
        /// Callback to clear the current server lgo.
        /// </summary>
        private void OnServerGuidChanged_UpdateConsoleLog(object? sender, EventArgs e)
        {
            ConsoleContents = string.Empty;
        }

        private bool IsTaskRunning(Task task)
        {
            return ApiConnection.Instance != null && (task != null && !task.IsCompleted);
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

        /// <summary>
        /// Processes a <see cref="ConsoleEntriesResponse"/> and updates <see cref="_ConsoleContents"/> with new contents. Has protections in place for out of order requests.
        /// </summary>
        /// <remarks>
        /// Must be run on the main thread.
        /// </remarks>
        /// <param name="response"></param>
        private void OnRequestConsoleUpdateSucceeded(ApiResponse<ConsoleEntriesResponse> response)
        {
            // Shouldn't need lock guard as it is invoked on the main thread.

            CheckInstanceAwareResponseForChange(response); // TODO: This should be called elsewhere.

            if (response.ErrorResult == Error.Ok && response.Response != null)
            {
                // Something went wrong with the response.
                if (response.Response.NewLines == null)
                        return;

                // sent wrong line
                if (response.Response.LineFrom != _ConsoleContentsLatestLogLine)
                    return;

                if (response.Response.NewLines.Count == 0 && _ConsoleContentsLatestLogLine == response.Response.LineTo)
                    return;

                foreach (var item in response.Response.NewLines)
                {
                    _ConsoleContents += item + "\n";
                }

                _ConsoleContentsLatestLogLine = response.Response.LineTo;
                NotifyFieldUpdated(nameof(ConsoleContents));
                ConsoleReadSuccessful?.Invoke(this, response.Response);
            }
        }


        /// <summary>
        /// Callback for when a command has been sent successfully.
        /// </summary>
        /// <remarks>
        /// Must be called on the main thread.
        /// </remarks>
        private void OnSendCommandComplete(ApiResponse<ConsoleCommandResponse> response)
        {
            SendCommandComplete?.Invoke(this, response);
        }


        /// <summary>
        /// Task for waiting the time defined by <see cref="Config.ServerPollIntervalMilliseconds"/>, then invoking <paramref name="onPeriodExceeded"/> on the current thread.
        /// </summary>
        private async Task PollServer(CancellationToken cancellationToken, Action onPeriodExceeded)
        {
            if (onPeriodExceeded == null)
            {
                Console.WriteLine("ERROR: Poll function undefined.");
                return;
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(Config.Instance.ServerPollIntervalMilliseconds, cancellationToken);
                await Application.Current.Dispatcher.BeginInvoke(onPeriodExceeded);
                if (this == null)
                    return;
            }
        }

        /// <summary>
        /// Callback for when <see cref="PollServer(CancellationToken, Action)"/> completes its period.
        /// </summary>
        /// <remarks>
        /// Must be called on the main thread.
        /// </remarkls>
        private void OnPollServerInterval()
        {
            TryRequestConsoleUpdate();
        }
    }
}
