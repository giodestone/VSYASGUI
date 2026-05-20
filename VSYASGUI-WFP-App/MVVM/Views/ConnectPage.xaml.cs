using System.Windows;
using System.Windows.Controls;
using VSYASGUI_WFP_App.MVVM.Views;

namespace VSYASGUI_WFP_App.Pages
{
    /// <summary>
    /// Interaction logic for ConnectPage.xaml
    /// </summary>
    internal sealed partial class ConnectPage : Page
    {
        public ConnectPage()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectingPage connectingPage = new ConnectingPage();
            this.NavigationService.Navigate(connectingPage);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Helpers.TryDestroyAllBackNavigation(NavigationService);
        }
    }
}
