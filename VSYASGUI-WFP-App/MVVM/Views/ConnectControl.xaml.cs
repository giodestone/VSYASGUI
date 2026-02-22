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

namespace VSYASGUI_WFP_App.MVVM.Views
{
    /// <summary>
    /// Interaction logic for ConnectControl.xaml
    /// </summary>
    internal sealed partial class ConnectControl : UserControl
    {
        public ConnectControl()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO FIX
            //Application.Current.MainWindow = new ConnectingControl();
        }
    }
}
