using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VSYASGUI_CommonLib.ResponseObjects;
using VSYASGUI_WFP_App.MVVM.Models;
using VSYASGUI_WFP_App.MVVM.ViewModels;
using VSYASGUI_WFP_App.MVVM.Views;

namespace VSYASGUI_WFP_App.UserControls
{
    /// <summary>
    /// Interaction logic for ServerConsole.xaml
    /// </summary>
    public partial class ServerConsole : UserControl
    {
        ConnectionPresenter _ConnectionPresenter;
        Task _PollServerTask;
        long _LatestLine = 0;

        public ServerConsole()
        {
            InitializeComponent();
        }

        private void OnPollServerInterval()
        {
            if (_ConnectionPresenter == null)
                return;

            _ConnectionPresenter.TryRequestConsoleUpdate(_LatestLine);
        }

        /// <summary>
        /// Task for waiting the time defined by <see cref="Config.ServerPollIntervalMilliseconds"/>, then invoking <paramref name="onPeriodExceeded"/> on the current thread.
        /// </summary>
        private async Task PollServer(UserControl target, Action onPeriodExceeded)
        {
            if (onPeriodExceeded == null)
            {
                Console.WriteLine("ERROR: Poll function undefined.");
                return;
            }

            while (true)
            {
                await Task.Delay(Config.Instance.ServerPollIntervalMilliseconds);
                await Application.Current.Dispatcher.BeginInvoke(onPeriodExceeded);
                if (this == null)
                    return;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _ConnectionPresenter = DataContext as ConnectionPresenter;
            if (_ConnectionPresenter == null)
            {
                Console.WriteLine("ERROR: Unable to find the data context. Will not begin tasks.");
                return;
            }

            _PollServerTask = PollServer(this, OnPollServerInterval);
            _ConnectionPresenter.ConsoleReadSuccessful += OnConsoleReadSuccessful;
            _ConnectionPresenter.ServerInstanceGuidChanged += OnServerGuidChanged;
            _ConnectionPresenter.SendCommandComplete += OnSendCommandComplete;
            
        }

        private void OnSendCommandComplete(object? sender, ApiResponse<ConsoleCommandResponse> e)
        {
            SendCommandButton.IsEnabled = true;
        }

        private void OnServerGuidChanged(object? sender, EventArgs e)
        {
            _LatestLine = 0;
            ConsoleTextBox.Clear();
        }

        private void OnConsoleReadSuccessful(object? sender, ConsoleEntriesResponse e)
        {             
            // Something went wrong with the response.
            if (e.NewLines == null)
                return;

            // sent wrong line
            if (e.LineFrom != _LatestLine)
                return;

            foreach (var item in e.NewLines)
            {
                ConsoleTextBox.Text += item + "\n";
            }

            _LatestLine = e.LineTo;
        }
    }
}
