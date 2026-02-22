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

namespace VSYASGUI_WFP_App.MVVM.Views
{
    /// <summary>
    /// Interaction logic for ConnectionFailedWindow.xaml
    /// </summary>
    public partial class ConnectionFailedWindow : Window
    {
        public string FailureReason { get; private set; }
        public string ResolutionSuggestions { get; private set; }

        /// <summary>
        /// Construct the connection failed page with default reasons.
        /// </summary>
        public ConnectionFailedWindow()
        { 
            FailureReason = "No reason given.";
            ResolutionSuggestions = "No resolution information given.";

            InitializeComponent();
        }

        /// <summary>
        /// Construct the page with the reasons.
        /// </summary>
        /// <param name="reason">Quick and snappy reason.</param>
        /// <param name="resolveSuggestions">Suggestion(s) as to how the user may resolve this.</param>
        public ConnectionFailedWindow(string reason, string resolveSuggestions)
        { 
            FailureReason = reason;
            ResolutionSuggestions = resolveSuggestions;

            InitializeComponent();
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow = new ConnectWindow();
        }
    }
}
