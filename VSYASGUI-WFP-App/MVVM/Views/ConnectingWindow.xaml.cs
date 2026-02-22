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
    /// Interaction logic for ConnectingWindow.xaml
    /// </summary>
    public partial class ConnectingWindow : Window
    {
        public ConnectingWindow()
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
            // TODO: Remove this from the navigation history, if forward/back navigation is ever implemented.

            switch (result)
            {
                case Error.Ok:
                    throw new NotImplementedException();
                case Error.Connection:
                    {
                        ConnectionFailedWindow connectionFailedWindow = new("Unable to establish connection to specified endpoint.", "Check the endpoitn address.");
                        Application.Current.MainWindow = connectionFailedWindow;
                    }
                    break;
                case Error.Unauthorised:
                    {
                        ConnectionFailedWindow connectionFailedWindow = new("Invalid API key.", "Check if the API key is the same one as the server you are trying to connect to.");
                        Application.Current.MainWindow =  connectionFailedWindow;
                    }
                    break;
                default:
                    {
                        ConnectionFailedWindow connectionFailedWindow = new("Failed to connect.", "Something went wrong when trying to communicate with the server.");
                        Application.Current.MainWindow =  connectionFailedWindow;
                    }
                    break;
            }
        }
    }
}
