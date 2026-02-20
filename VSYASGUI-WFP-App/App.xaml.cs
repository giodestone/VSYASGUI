using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Windows;
using VSYASGUI_WFP_App.MVVM.Models;
using VSYASGUI_WFP_App.MVVM.ViewModels;

namespace VSYASGUI_WFP_App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            Config.Instance.TrySave();

            base.OnExit(e);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (Config.TryLoadOrCreate() == false)
            {
                MessageBox.Show(Config.Instance.FailedToCreateOrLoadConfigText, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

}
