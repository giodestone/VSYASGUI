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

namespace VSYASGUI_WFP_App.UserControls
{
    /// <summary>
    /// Interaction logic for MenuBar.xaml
    /// </summary>
    public partial class MenuBar : UserControl
    {
        public MenuBar()
        {
            InitializeComponent();
        }

        private void ExitCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    
        private void ClearConfigurationCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ClearConfigCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO: Make this flow a bit less awful and capable of giving multiple errors. Does the first error even matter to the user?
            bool hadProblem = false;
            if (Config.Instance.TryDeleteConfig() == false)
            {
                MessageBox.Show("Failed to delete existing config. Check you have permissions to delete the file, or that it was created in the first place.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                hadProblem = true;
            }

            if (Config.TryLoadOrCreate() == false)
            {
                MessageBox.Show(Config.Instance.FailedToCreateOrLoadConfigText, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                hadProblem = true;
            }

            if (!hadProblem)
            {
                MessageBox.Show("Configuration reset successfully.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    public static class MenuBarCommands
    {
        public static readonly RoutedUICommand ExitCommand = new RoutedUICommand("Exit", "Exit", typeof(MenuBarCommands), new InputGestureCollection());
        public static readonly RoutedUICommand ClearConfigurationCommand = new RoutedUICommand("Clear Configuration", "Clear Configuration", typeof(MenuBarCommands), new InputGestureCollection());
    }
}
