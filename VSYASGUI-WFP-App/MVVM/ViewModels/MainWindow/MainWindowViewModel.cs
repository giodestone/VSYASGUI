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

        public ICommand ChangeToConnectScreenCommand => return new Rou
        //public ICommand ChangeToFirstViewCommand => 

        public MainWindowViewModel()
        {

        }

        public void SwichViews(MainWindowViews newView)
        {

        }


    }
}
