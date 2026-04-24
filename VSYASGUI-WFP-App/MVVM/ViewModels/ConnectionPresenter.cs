using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VSYASGUI_CommonLib;
using VSYASGUI_CommonLib.RequestObjects;
using VSYASGUI_CommonLib.ResponseObjects;
using VSYASGUI_WFP_App.MVVM.Models;
using VSYASGUI_WFP_App.MVVM.ViewModels.Base;

namespace VSYASGUI_WFP_App.MVVM.ViewModels
{
    /// <summary>
    /// View Model for various connection related operations, including checking the connection, updating the console, and player operations.
    /// </summary>
    internal sealed class ConnectionPresenter : Presenter
    {
        private const string _Unavailable = "Unavailable";
        private const string _NoGroups = "No group membership";

        public event EventHandler ConnectionCheckBegun;
        public event EventHandler<Error> ConnectionCheckComplete;
        
        private CancellationTokenSource _ConnectionCheckCancellationTokenSource;
        private Task<ApiResponse<ConnectionCheckResponse>> _CurrentConnectionCheckTask;

        /// <summary>
        /// Called when the received server instance GUID changes.
        /// </summary>
        public event EventHandler ServerInstanceGuidChanged;
        private Guid _LatestServerGuid = Guid.Empty;

        /// <summary>
        /// Called when command details completes. Make sure to check the ApiResponse.
        /// </summary>
        public event EventHandler<ApiResponse<ConsoleCommandResponse>> SendCommandComplete;
        private Task<ApiResponse<ConsoleCommandResponse>> _SendCommandTask;
        private CancellationTokenSource _SendCommandCancelleationToken;
        private string _SendCommandContents = string.Empty;

        private Task _PollServerTask;
        private CancellationTokenSource _PollServerCancellationTokenSource;
        
        /// <summary>
        /// Called when the console read task completes.
        /// </summary>
        public event EventHandler<ConsoleEntriesResponse> ConsoleReadSuccessful;
        private Task<ApiResponse<ConsoleEntriesResponse>> _ConsoleEntryRequestTask;
        private long _ConsoleContentsLatestLogLine = 0;
        private string _ConsoleContents = string.Empty;

        /// <summary>
        /// Called when the server statistics (cpu usage, memory etc.) have changed.
        /// </summary>
        public event EventHandler<ApiResponse<ServerStatisticsResponse>> ServerStatisticsUpdated;
        private Task<ApiResponse<ServerStatisticsResponse>> _ServerStatisticsUpdateTask;
        private ServerStatisticsResponse? _LatestServerStatisticsResponse = null;
        private string _ServerStatus = _Unavailable;
        private int _NumRequestsFailed = 0;

        /// <summary>
        /// Called when the player overviews change (usually when a new player is sent).
        /// </summary>
        public event EventHandler<ApiResponse<PlayerOverviewResponse>> PlayerOverviewChanged;
        private Task<ApiResponse<PlayerOverviewResponse>> _PlayerOverviewRequestTask;
        private CancellationTokenSource _PlayerOverviewCancellationTokenSource;
        private string _PreviousPlayerOverviewHash = string.Empty;
        private ObservableCollection<PlayerOverview> _PlayerOverviews = new ObservableCollection<PlayerOverview>();
        private int _SelectedPlayerOverviewIndex = -1;


        private Task<ApiResponse<NoResponse>> _DownloadFileRequestTask;
        private CancellationTokenSource _DownloadFileCancellationTokenSource;

        /// <summary>
        /// Command form of <see cref="TryBeginConnectionCheck"/>.
        /// </summary>
        public ICommand TryBeginConnectionCheckCommand => new Command(_ => TryBeginConnectionCheck());

        /// <summary>
        /// Command form of <see cref="TryCancelConnectionCheck"/>.
        /// </summary>
        public ICommand TryCancelConnectionCheckCommand => new Command(_ => TryCancelConnectionCheck());

        /// <summary>
        /// Command form of <see cref="TrySendConsoleCommand"/>
        /// </summary>
        public ICommand TrySendCommandToServerCommand => new Command(_ => TrySendConsoleCommand());

        /// <summary>
        /// Command form of <see cref="TryKickCurrentlySelectedPlayer"/>.
        /// </summary>
        public ICommand TryKickCurrentlySelectedPlayerCommand => new Command(_ => TryKickCurrentlySelectedPlayer());

        /// <summary>
        /// Command form of <see cref="TryBanCurrentlySelectedPlayer"/>.
        /// </summary>
        public ICommand TryBanCurrentlySelectedPlayerCommand => new Command(_ => TryBanCurrentlySelectedPlayer());

        /// <summary>
        /// Command form of <see cref="TryUnbanCurrentlySelectedPlayer"/>.
        /// </summary>
        public ICommand TryUnbanCurrentlySelectedPlayerCommand => new Command(_ => TryUnbanCurrentlySelectedPlayer());

        public ICommand TryRequestWorldSaveCommand => new Command(_ => TryRequestWorldSave());



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
        /// Alias for <see cref="_LatestServerStatisticsResponse"/>'s <see cref="ServerStatisticsResponse.CpuUsagePercentage"/> (with formatting). Returns <see cref="_Unavailable"/> if no update has been recieved.
        /// </summary>
        public string CpuUsagePercentage
        {
            get
            {
                if (_LatestServerStatisticsResponse == null)
                    return _Unavailable;
                return _LatestServerStatisticsResponse.CpuUsagePercentage.ToString("F1");
            }
        }

        /// <summary>
        /// Wrapper for <see cref="_LatestServerStatisticsResponse"/>'s <see cref="ServerStatisticsResponse.MemoryUsageBytes"/> (with conversion). Returns <see cref="_Unavailable"/> if no update has been recieved.
        /// </summary>
        public string MemoryUsageMb
        {
            get
            {
                if (_LatestServerStatisticsResponse == null)
                    return _Unavailable;
                return (_LatestServerStatisticsResponse.MemoryUsageBytes / 1024 / 1024).ToString("F0");
            }
        }

        /// <summary>
        /// Alias for <see cref="_LatestServerStatisticsResponse"/>'s <see cref="ServerStatisticsResponse.ServerSecondsUptime"/> (with conversion to human time). Returns <see cref="_Unavailable"/> if no update has been recieved.
        /// </summary>
        public string ServerUptime
        {
            get
            {
                if (_LatestServerStatisticsResponse == null)
                    return _Unavailable;
                return TimeSpan.FromSeconds(_LatestServerStatisticsResponse.ServerSecondsUptime).ToString();
            }
        }

        /// <summary>
        ///         /// <summary>
        /// Alias for <see cref="_LatestServerStatisticsResponse"/>'s <see cref="ServerStatisticsResponse.TotalWorldPlaytime"/> (with conversion to human time). Returns <see cref="_Unavailable"/> if no update has been recieved.
        /// </summary>
        /// </summary>
        public string TotalWorldPlaytime
        {
            get
            {
                if (_LatestServerStatisticsResponse == null)
                    return _Unavailable;
                return TimeSpan.FromSeconds(_LatestServerStatisticsResponse.TotalWorldPlaytime).ToString();
            }
        }

        /// <summary>
        /// Alias for <see cref="_LatestServerStatisticsResponse"/>'s <see cref="ServerStatisticsResponse.OnlinePlayerCount"/>. Returns <see cref="_Unavailable"/> if no update has been recieved.
        /// </summary>
        public string OnlinePlayerCount
        {
            get
            {
                if (_LatestServerStatisticsResponse == null)
                    return _Unavailable;
                return _LatestServerStatisticsResponse.OnlinePlayerCount.ToString();
            }
        }

        /// <summary>
        /// All player overviews currently avaliable. Due to a technicality of the VS API, will only provide players that connected since last server restart.
        /// </summary>
        public ObservableCollection<PlayerOverview> PlayerOverviews
        {
            get
            {
                return _PlayerOverviews;
            }
            set
            {
                UpdateFieldWithValue(ref _PlayerOverviews, value, nameof(PlayerOverviews));
            }
        }

        /// <summary>
        /// Tracks which currently selected player overview is selected.
        /// </summary>
        public int SelectedPlayerOverviewIndex
        {
            get
            {
                return _SelectedPlayerOverviewIndex;
            }
            set
            {
                UpdateFieldWithValue(ref _SelectedPlayerOverviewIndex, value, nameof(SelectedPlayerOverviewIndex));
                NotifyFieldUpdated(nameof(SelectedPlayerName));
                NotifyFieldUpdated(nameof(SelectedPlayerUid));
                NotifyFieldUpdated(nameof(SelectedPlayerFirstJoinDate));
                NotifyFieldUpdated(nameof(SelectedPlayerLastJoinDate));
                NotifyFieldUpdated(nameof(SelectedPlayerLastKnownName));
                NotifyFieldUpdated(nameof(SelectedPlayerGroups));
                NotifyFieldUpdated(nameof(CanSendPlayerActionCommands));
            }
        }

        /// <summary>
        /// Get the currently selected player overview, as per the <see cref="SelectedPlayerOverviewIndex"/>.
        /// </summary>
        private PlayerOverview? SelectedPlayerOverview
        {
            get
            {
                if (SelectedPlayerOverviewIndex < 0 || SelectedPlayerOverviewIndex >= _PlayerOverviews.Count)
                    return null;

                return PlayerOverviews[SelectedPlayerOverviewIndex];
            }
        }

        /// <summary>
        /// Alias for <see cref="_SelectedPlayerOverviewIndex"/>'s <see cref="PlayerOverview.Name"/>. Returns <see cref="_Unavailable"/> if nothing is selected or string is empty/null.
        /// </summary>
        public string SelectedPlayerName
        {
            get
            {
                if (SelectedPlayerOverview == null || string.IsNullOrEmpty(SelectedPlayerOverview.Name))
                    return _Unavailable;

                return SelectedPlayerOverview.Name;
            }
        }

        /// <summary>
        /// Alias for <see cref="_SelectedPlayerOverviewIndex"/>'s <see cref="PlayerOverview.PlayerUid"/>. Returns <see cref="_Unavailable"/> if nothing is selected or string is empty/null.
        /// </summary>
        public string SelectedPlayerUid
        {
            get
            {
                if (SelectedPlayerOverview == null || string.IsNullOrEmpty(SelectedPlayerOverview.PlayerUid))
                    return _Unavailable;

                return SelectedPlayerOverview.PlayerUid;
            }
        }

        /// <summary>
        /// Alias for <see cref="_SelectedPlayerOverviewIndex"/>'s <see cref="PlayerOverview.FirstJoinDate"/>. Returns <see cref="_Unavailable"/> if nothing is selected or string is empty/null.
        /// </summary>
        public string SelectedPlayerFirstJoinDate
        {
            get
            {
                if (SelectedPlayerOverview == null || string.IsNullOrEmpty(SelectedPlayerOverview.FirstJoinDate))
                    return _Unavailable;

                return SelectedPlayerOverview.FirstJoinDate;
            }
        }

        /// <summary>
        /// Alias for <see cref="_SelectedPlayerOverviewIndex"/>'s <see cref="PlayerOverview.LastJoinDate"/>. Returns <see cref="_Unavailable"/> if nothing is selected or string is empty/null.
        /// </summary>
        public string SelectedPlayerLastJoinDate
        {
            get
            {
                if (SelectedPlayerOverview == null || string.IsNullOrEmpty(SelectedPlayerOverview.LastJoinDate))
                    return _Unavailable;

                return SelectedPlayerOverview.LastJoinDate;
            }
        }

        /// <summary>
        /// Alias for <see cref="_SelectedPlayerOverviewIndex"/>'s <see cref="PlayerOverview.LastKnownName"/>. Returns <see cref="_Unavailable"/> if nothing is selected or string is empty/null.
        /// </summary>
        public string SelectedPlayerLastKnownName
        {
            get
            {
                if (SelectedPlayerOverview == null || string.IsNullOrEmpty(SelectedPlayerOverview.LastKnownName))
                    return _Unavailable;

                return SelectedPlayerOverview.LastKnownName;
            }
        }

        /// <summary>
        /// Alias for <see cref="_SelectedPlayerOverviewIndex"/>'s <see cref="PlayerOverview.Groups"/>. Returns <see cref="_Unavailable"/> if nothing is selected.
        /// </summary>
        public string SelectedPlayerGroups
        {
            get
            {
                if (SelectedPlayerOverview == null)
                    return _Unavailable;

                if (string.IsNullOrEmpty(SelectedPlayerOverview.Groups))
                    return _NoGroups;

                return SelectedPlayerOverview.Groups;
            }
        }
        
        /// <summary>
        /// Whether the player actions on the Player Overview menu (kick/ban/unban) should be possible to execute.
        /// </summary>
        public bool CanSendPlayerActionCommands
        {
            get
            {
                if (SelectedPlayerOverview == null)
                    return false;

                return CanSendConsoleCommand();
            }
        }

        public string ServerStatus
        {
            get
            {
                return _ServerStatus;
            }
        }

        /// <summary>
        /// Calls <see cref="ApiConnection.SetupConnection(string, string)"/> if <see cref="ApiConnection.Instance"/> is already null.
        /// </summary>
        public ConnectionPresenter()
        {
            if (ApiConnection.Instance == null)
                ApiConnection.SetupConnection(Config.Instance.GetUrlForApi);

            BeginPeriodicPolling();
            ServerInstanceGuidChanged += OnServerGuidChanged_UpdateConsoleLog;
        }

        /// <summary>
        /// Begins the polling task (<see cref="_PollServerTask"/>) which takes volatile info from the server.
        /// </summary>
        public void BeginPeriodicPolling()
        {
            if (_PollServerCancellationTokenSource != null)
                _PollServerCancellationTokenSource.Cancel();
            _PollServerCancellationTokenSource = new CancellationTokenSource();
            _PollServerTask = PollServer(_PollServerCancellationTokenSource.Token, OnPollServerInterval);
        }

        /// <summary>
        /// Stops the polling task (<see cref="_PollServerTask"/>) which takes certain info from the server.
        /// </summary>
        public void StopPeriodicPolling()
        {
            _PollServerCancellationTokenSource.Cancel();
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
        /// Whether a console command can be sent (i.e. one is not currently being sent).
        /// </summary>
        public bool CanSendConsoleCommand()
        {
            if (_ConsoleEntryRequestTask == null)
                return true;
            if (!_ConsoleEntryRequestTask.IsCompleted)
                return false;

            return true;
        }

        /// <summary>
        /// Try to send the command set in <see cref="SendCommandContents"/> to the server.
        /// </summary>
        /// <returns><c>true</c> if a command has been sent. <c>false</c> if not, due to the value of <see cref="CanSendConsoleCommand"/> or technical Error.</returns>
        public bool TrySendConsoleCommand()
        {
            if (!CanSendConsoleCommand())
            {
                NotifyFieldUpdated(nameof(CanSendPlayerActionCommands));
                return false;
            }

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

                NotifyFieldUpdated(nameof(CanSendPlayerActionCommands));
                return false;
            }

            NotifyFieldUpdated(nameof(CanSendPlayerActionCommands));
            return true;
        }

        /// <summary>
        /// Try to get the current value of the server console. Expects to be called from <see cref="OnPollServerInterval"/>.
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

            NotifyFieldUpdated(nameof(CanSendPlayerActionCommands));
        }


        /// <summary>
        /// Task for waiting the time defined by <see cref="Config.ServerPollIntervalMs"/>, then invoking <paramref name="onPeriodExceeded"/> on the current thread.
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
                await Task.Delay(Config.Instance.ServerPollIntervalMs, cancellationToken);
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
            if (TryRequestServerStatisticsUpdate() != Error.Ok)
            {
                _ServerStatus = "Status check failed";
                NotifyFieldUpdated(nameof(ServerStatus));
            }

            TryRequestConsoleUpdate();
            TryRequestPlayersUpdate();
        }

        /// <summary>
        /// Try to request a server update from the tasks. Expects to be called from <see cref="OnPollServerInterval"/>.
        /// </summary>
        /// <returns>true if it was requested, false otherwise.</returns>
        private Error TryRequestServerStatisticsUpdate()
        {
            return TryMakeRequest(ref _ServerStatisticsUpdateTask, _PollServerCancellationTokenSource, new ServerStatisticsRequest(), OnServerStatisticsUpdateSucceeded);
        }

        /// <summary>
        /// Callback for when a <see cref="_ServerStatisticsUpdateTask"/> completes. Updates currently held statistics value <see cref="_LatestServerStatisticsResponse"/> and notifies any relevant fields.
        /// </summary>
        /// <remarks>
        /// Must be run on the main thread.
        /// </remarks>
        private void OnServerStatisticsUpdateSucceeded(ApiResponse<ServerStatisticsResponse> response)
        {
            UpdateServerStatus(response);

            _LatestServerStatisticsResponse = response.Response;
            NotifyServerOverviewFieldsUpdated();

            ServerStatisticsUpdated?.Invoke(this, response);
        }

        /// <summary>
        /// Updates <see cref="ServerStatus"/> based on response given by a server statistics update.
        /// </summary>
        /// <param name="response"></param>
        private void UpdateServerStatus(ApiResponse<ServerStatisticsResponse> response)
        {
            if (response.ErrorResult == Error.Ok)
            {
                _ServerStatus = "Running";
                NotifyFieldUpdated(nameof(ServerStatus));
            }
            else if (response.ErrorResult != Error.Ok)
            {
                if (response.ErrorResult == Error.RequestAlreadyInProgress)
                {
                    _NumRequestsFailed++;

                    if (_NumRequestsFailed >= Config.Instance.MaxFailedConnectionRequests)
                    {
                        _ServerStatus = "Unable to reach server";
                        _NumRequestsFailed = 0;
                        NotifyFieldUpdated(nameof(ServerStatus));
                    }

                    return;
                }

                _ServerStatus = "Failed to check status";
                NotifyFieldUpdated(nameof(ServerStatus));

                return;
            }
        }


        /// <summary>
        /// Notify that fields related to the server overview (cpu usage, ram usage) have been updated using <see cref="Presenter.NotifyFieldUpdated(string?)"/>.
        /// </summary>
        private void NotifyServerOverviewFieldsUpdated()
        {
            NotifyFieldUpdated(nameof(CpuUsagePercentage));
            NotifyFieldUpdated(nameof(MemoryUsageMb));
            NotifyFieldUpdated(nameof(ServerUptime));
            NotifyFieldUpdated(nameof(TotalWorldPlaytime));
            NotifyFieldUpdated(nameof(OnlinePlayerCount));
        }

        /// <summary>
        /// Attempts to make a request to get a new set of player overviews using <see cref="TryMakeRequest{TResponse}(ref Task{ApiResponse{TResponse}}, CancellationTokenSource, RequestBase, Action{ApiResponse{TResponse}})"/>. Calls <see cref="OnPlayerOverviewRequestComplete(ApiResponse{PlayerOverviewResponse})"/> upon completion.
        /// <br/><br/>
        /// Expects to be called from <see cref="OnPollServerInterval"/>.
        /// </summary>
        /// <returns><c>true</c> if request was made, <c>false</c> if it was not.</returns>
        private Error TryRequestPlayersUpdate()
        {
            return TryMakeRequest(ref _PlayerOverviewRequestTask, _PollServerCancellationTokenSource, new PlayerOverviewRequest(), OnPlayerOverviewRequestComplete);
        }

        /// <summary>
        /// Callback for when <see cref="_PlayerOverviewRequestTask"/> completes.
        /// 
        /// Updates <see cref="PlayerOverviews"/> and <see cref="_PreviousPlayerOverviewHash"/> (only if recieved hash is mismatched).
        /// </summary>
        /// <remarks>
        /// Must be run on the main thread.
        /// </remarks>
        /// <param name="response"></param>
        private void OnPlayerOverviewRequestComplete(ApiResponse<PlayerOverviewResponse> response)
        {
            if (response.ErrorResult != Error.Ok)
                return;

            if (response.Response.PlayerOverviews == null)
            {
                PlayerOverviews = new ObservableCollection<PlayerOverview>();
                Console.Error.WriteLine("Recieved a null PlayerOverview when it should not have been null! Will not update Player overview data.");
                return;
            }

            if (response.Response.HashOfPlayerOverviews == _PreviousPlayerOverviewHash)
                return;

            _PreviousPlayerOverviewHash = response.Response.HashOfPlayerOverviews; // prevents unneccessary updates.
            PlayerOverviews = new ObservableCollection<PlayerOverview>(response.Response.PlayerOverviews);

            NotifyFieldUpdated(nameof(PlayerOverviews));
        }

        /// <summary>
        /// Try to make a request in a wrapped way with <see cref="ApiConnection.RequestApiInfo{TExpectedResponse}(RequestBase, CancellationToken)"/>.
        /// </summary>
        /// <typeparam name="TResponse">Type of the expected response.</typeparam>
        /// <param name="baseTask">Where the launched task gets stored.</param>
        /// <param name="cancellationTokenSource">Cancellation token.</param>
        /// <param name="request">The request to send. Will auto-populate <see cref="RequestBase.ApiKey"/> with <see cref="ApiConnection.RequestApiInfo{TExpectedResponse}(RequestBase, CancellationToken)"/>.</param>
        /// <param name="continuationFunction">Function to call when the request completes. Invoked by the main thread's dispatcher, as defined by <see cref="Application.Current"/>.</param>
        /// <returns><c>true</c> if the request was made, <c>false</c> if it cannot be made either due to an exception or outcome of <see cref="IsTaskRunning(Task)"/>.</returns>
        private Error TryMakeRequest<TResponse>(ref Task<ApiResponse<TResponse>> baseTask, CancellationTokenSource cancellationTokenSource, RequestBase request, Action<ApiResponse<TResponse>> continuationFunction) where TResponse : ResponseBase, new()
        {
            if (IsTaskRunning(baseTask))
                return Error.RequestAlreadyInProgress;

            try
            {
                if (cancellationTokenSource == null)
                    cancellationTokenSource = new CancellationTokenSource();

                baseTask = ApiConnection.Instance.RequestApiInfo<TResponse>(request, cancellationTokenSource.Token);
                baseTask.ContinueWith(task => Application.Current.Dispatcher.BeginInvoke(continuationFunction, task.Result));
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: Unable to invoke task due to an exception.");
                Console.WriteLine(e.Message);
                return Error.General;
            }

            return Error.Ok;
        }

        /// <summary>
        /// Try to run the /kick command on the currently selected player, if possible.
        /// </summary>
        /// <returns>Returns <c>true</c> if it can be run, <c>false</c> if not, depending on <see cref="CanSendPlayerActionCommands"/> and <see cref="TrySendConsoleCommand"/>.</returns>

        private bool TryKickCurrentlySelectedPlayer()
        {
            if (!CanSendPlayerActionCommands)
                return false;

            string previousConsoleContents = _SendCommandContents;
            _SendCommandContents = $"/kick {SelectedPlayerOverview.Name}";
            
            bool returnVal = TrySendConsoleCommand();

            _SendCommandContents = previousConsoleContents;

            return returnVal;
        }

        /// <summary>
        /// Try to run the /ban command on the currently selected player, if possible.
        /// </summary>
        /// <returns>Returns <c>true</c> if it can be run, <c>false</c> if not, depending on <see cref="CanSendPlayerActionCommands"/> and <see cref="TrySendConsoleCommand"/>.</returns>
        private bool TryBanCurrentlySelectedPlayer()
        {
            if (!CanSendPlayerActionCommands)
                return false;

            string previousConsoleContents = _SendCommandContents;
            _SendCommandContents = $"/ban {SelectedPlayerOverview.Name} 100 year No reason specified.";

            bool returnVal = TrySendConsoleCommand();

            _SendCommandContents = previousConsoleContents;

            return returnVal;
        }

        /// <summary>
        /// Try to run the /unban command on the currently selected player, if possible.
        /// </summary>
        /// <returns>Returns <c>true</c> if it can be run, <c>false</c> if not, depending on <see cref="CanSendPlayerActionCommands"/> and <see cref="TrySendConsoleCommand"/>.</returns>
        private bool TryUnbanCurrentlySelectedPlayer()
        {
            if (!CanSendPlayerActionCommands)
                return false;

            string previousConsoleContents = _SendCommandContents;
            _SendCommandContents = $"/unban {SelectedPlayerOverview.Name}";

            bool returnVal = TrySendConsoleCommand();

            _SendCommandContents = previousConsoleContents;

            return returnVal;
        }

        private bool TryRequestWorldSave()
        {
            _DownloadFileCancellationTokenSource = new CancellationTokenSource();
            _DownloadFileRequestTask = ApiConnection.Instance.RequestApiInfo<NoResponse>(new WorldDownloadRequest(), _DownloadFileCancellationTokenSource.Token);
            //_DownloadFileRequestTask.ContinueWith(task => Application.Current.Dispatcher.BeginInvoke(OnWorldDownloadComplete, task.Result));

            return true;
        }

        private void OnWorldDownloadComplete()
        {

        }
    }
}
