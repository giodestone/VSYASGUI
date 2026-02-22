using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VSYASGUI_WFP_App.MVVM.ViewModels.MainWindow;

namespace VSYASGUI_WFP_App.MVVM.ViewModels
{

    internal sealed class MainWindowViewModel : ViewModels.Base.Presenter
    {
        private MainWindowViews _CurrentView = MainWindowViews.Default;

        //public ICommand ChangeToConnectScreenCommand => return 
        //public ICommand ChangeToFirstViewCommand => 

        public void SwichViews(MainWindowViews newView)
        {

        }


    }
}
