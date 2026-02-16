using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VSYASGUI_WFP_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (Config.LoadOrCreate() == false)
            {
                MessageBox.Show($"Unable to load or create the configuration file. \n\nNo user settings will be remembered such as the API keys. \n\nTry removing the config file at ({Config.GetPathToConfig()}), checking if your disk is full, that you have write permissions, or has been broken.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}