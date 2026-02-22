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

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Helpers.DestroyAllBackNavigation(NavigationService);
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectingPage connectingPage = new ConnectingPage();
            this.NavigationService.Navigate(connectingPage);
        }
    }
}
