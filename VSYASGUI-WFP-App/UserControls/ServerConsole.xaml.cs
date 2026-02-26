using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class ServerConsole : UserControl, INotifyPropertyChanged
    {
        ConnectionPresenter _ConnectionPresenter;

        private bool _AutomaticallyScrollToBottom = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool AutomaticallyScrollToBottom
        {
            get => _AutomaticallyScrollToBottom;
            set
            {
                _AutomaticallyScrollToBottom = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutomaticallyScrollToBottom)));
            }
        }

        public ServerConsole()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _ConnectionPresenter = DataContext as ConnectionPresenter;
            if (_ConnectionPresenter == null)
            {
                Console.WriteLine("ERROR: Unable to find the data context. Will not begin tasks.");
                return;
            }

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
            ConsoleTextBox.Clear();
        }

        private void OnConsoleReadSuccessful(object? sender, ConsoleEntriesResponse e)
        {             
            if (AutomaticallyScrollToBottom)
            {
                ConsoleTextBox.UpdateLayout();
                ConsoleTextBox.ScrollToEnd();
            }
        }
    }
}
