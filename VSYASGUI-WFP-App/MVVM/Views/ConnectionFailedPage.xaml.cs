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
using VSYASGUI_WFP_App.Pages;

namespace VSYASGUI_WFP_App.MVVM.Views
{
    /// <summary>
    /// Interaction logic for ConnectionFailedPage.xaml
    /// </summary>
    public partial class ConnectionFailedPage : Page
    {
        private const string ReasonDefaultText = "No reason given.";
        private const string ResolutionDefaultText = "No resolution information given.";

        public string FailureReason { get; private set; } = ReasonDefaultText;
        public string ResolutionSuggestions { get; private set; } = ResolutionDefaultText;

        /// <summary>
        /// Construct the connection failed page with default reasons.
        /// </summary>
        public ConnectionFailedPage()
        { 
            InitializeComponent();
        }

        /// <summary>
        /// Construct the page with the reasons.
        /// </summary>
        /// <param name="reason">Quick and snappy reason.</param>
        /// <param name="resolveSuggestions">Suggestion(s) as to how the user may resolve this.</param>
        public ConnectionFailedPage(string reason, string resolveSuggestions)
        { 
            if (!string.IsNullOrEmpty(reason))
                FailureReason = reason;
            if (!string.IsNullOrEmpty(resolveSuggestions))
                ResolutionSuggestions = resolveSuggestions;

            InitializeComponent();
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ConnectPage());
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Helpers.TryDestroyAllBackNavigation(NavigationService);
        }
    }
}
