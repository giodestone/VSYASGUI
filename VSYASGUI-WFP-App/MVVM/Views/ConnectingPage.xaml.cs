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
using VSYASGUI_WFP_App.MVVM.Models;
using VSYASGUI_WFP_App.MVVM.ViewModels;
using VSYASGUI_WFP_App.Pages;

namespace VSYASGUI_WFP_App.MVVM.Views
{
    /// <summary>
    /// Interaction logic for ConnectingPage.xaml
    /// </summary>
    internal partial class ConnectingPage : Page
    {
        ConnectionPresenter? _ConnectionPresenter;

        public ConnectingPage()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        /// <summary>
        /// Check the connection to the server.
        /// </summary>
        private void CheckConnection()
        {
            _ConnectionPresenter?.TryBeginConnectionCheck();



            //ApiConnection.SetupConnection(Config.Instance.CurrentEndpoint, Config.Instance.CurrentApiKey);

            //// Fixes issue with the code in OnCheckConnectionComplete below being ran on the wrong thread.
            //var t = ApiConnection.Instance.CheckConnection().ContinueWith(
            //    task => Application.Current.Dispatcher.BeginInvoke(OnCheckConnectionComplete, task.Result
            
        }

        /// <summary>
        /// Moves onto the next screen for when the <see cref="ApiConnection.CheckConnection"/> operation completes.
        /// </summary>
        private void OnCheckConnectionComplete(object? sender, Error result)
        {
            switch (result)
            {
                case Error.Ok:
                    {
                        ServerPage serverPage = new();
                        NavigationService.Navigate(serverPage);
                    }
                    break;
                case Error.Connection:
                    {
                        ConnectionFailedPage connectionFailedPage = new("Unable to establish connection to specified endpoint.", "Check the endpoint address.");
                        NavigationService.Navigate(connectionFailedPage);
                    }
                    break;
                case Error.Unauthorised:
                    {
                        ConnectionFailedPage connectionFailedPage = new("Invalid API key.", "Check if the API key is the same one as the server you are trying to connect to.");
                        NavigationService.Navigate(connectionFailedPage);
                    }
                    break;
                case Error.Cancelled:
                    {
                        ConnectionFailedPage connectPage = new("User cancelled connection attempt.", string.Empty);
                        NavigationService.Navigate(connectPage);
                    }
                    break;
                default:
                    {
                        ConnectionFailedPage connectionFailedPage = new("Failed to connect.", "Something went wrong when trying to communicate with the server.");
                        NavigationService.Navigate(connectionFailedPage);
                    }
                    break;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Helpers.TryDestroyAllBackNavigation(NavigationService);

            ConnectionPresenter connectionPresenter = DataContext as ConnectionPresenter;
            if (connectionPresenter == null)
            {
                Console.WriteLine("ERROR: Data context is of an invalid (unexpected) type.");
                return;
            }
             _ConnectionPresenter = connectionPresenter;
            _ConnectionPresenter.ConnectionCheckComplete += OnCheckConnectionComplete;

            CheckConnection();
        }
    }
}
