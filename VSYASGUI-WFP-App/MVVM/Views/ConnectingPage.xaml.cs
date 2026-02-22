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

namespace VSYASGUI_WFP_App.MVVM.Views
{
    /// <summary>
    /// Interaction logic for ConnectingPage.xaml
    /// </summary>
    public partial class ConnectingPage : Page
    {
        public ConnectingPage()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            CheckConnection();
        }

        /// <summary>
        /// Check the connection to the server.
        /// </summary>
        private void CheckConnection()
        {
            ApiConnection.SetupConnection(Config.Instance.CurrentEndpoint, Config.Instance.CurrentApiKey);

            // Fixes issue with the code in OnCheckConnectionComplete below being ran on the wrong thread.
            ApiConnection.Instance.CheckConnection().ContinueWith(
                task => Application.Current.Dispatcher.BeginInvoke(OnCheckConnectionComplete, task.Result));
        }

        /// <summary>
        /// Moves onto the next screen for when the <see cref="ApiConnection.CheckConnection"/> operation completes.
        /// </summary>
        [STAThread]
        private void OnCheckConnectionComplete(Error result)
        {
            switch (result)
            {
                case Error.Ok:
                    throw new NotImplementedException();
                case Error.Connection:
                    {
                        ConnectionFailedPage connectionFailedPage = new("Unable to establish connection to specified endpoint.", "Check the endpoitn address.");
                        NavigationService.Navigate(connectionFailedPage);
                    }
                    break;
                case Error.Unauthorised:
                    {
                        ConnectionFailedPage connectionFailedPage = new("Invalid API key.", "Check if the API key is the same one as the server you are trying to connect to.");
                        NavigationService.Navigate(connectionFailedPage);
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
        }
    }
}
