using System.Collections.ObjectModel;
using System.Windows.Input;
using VSYASGUI_WFP_App.MVVM.Models;
using VSYASGUI_WFP_App.MVVM.ViewModels.Base ;

namespace VSYASGUI_WFP_App.MVVM.ViewModels
{
    public sealed class ConfigPresenter : Presenter
    {
        private string _CurrentlySelectedApiKey;
        private ObservableCollection<string> _ApiKeyHistory;

        private string _CurrentlySelectedEndpoint;
        private ObservableCollection<string> _EndpointHistory;

        public ObservableCollection<string> ApiKeyHistory => _ApiKeyHistory;
        public ObservableCollection<string> EndpointHistory => _EndpointHistory;

        public string CurrentlySelectedApiKey 
        { 
            get => _CurrentlySelectedApiKey; 
            set 
            {
                Update(ref _CurrentlySelectedApiKey, value);
                _CurrentlySelectedApiKey = value;
            }
        }

        public string CurrentlySelectedEndpoint
        {
            get => _CurrentlySelectedEndpoint;
            set
            {
                Update(ref _CurrentlySelectedEndpoint, value);
                _CurrentlySelectedEndpoint = value;
            }
        }

        public ConfigPresenter()
        {
            InitObservableCollections();

            if (Config.Instance.ApiKeys.Count > 0)
                _CurrentlySelectedApiKey = Config.Instance.ApiKeys[0];

            if (Config.Instance.EndpointAddresses.Count > 0)
                _CurrentlySelectedEndpoint = Config.Instance.EndpointAddresses[0];
        }

        private void InitObservableCollections()
        {
            _ApiKeyHistory = new ObservableCollection<string>(Config.Instance.ApiKeys);
            _EndpointHistory = new ObservableCollection<string>(Config.Instance.EndpointAddresses);
        }

        public ICommand ConnectCommand
        {
            get
            {
                return new Command(_ =>
                {
                    if (!Config.Instance.ApiKeys.Contains(_CurrentlySelectedApiKey) && !string.IsNullOrEmpty(_CurrentlySelectedApiKey))
                    {
                        Config.Instance.ApiKeys.Insert(0, _CurrentlySelectedApiKey);
                        ApiKeyHistory.Insert(0, _CurrentlySelectedApiKey);
                    }

                    if (!Config.Instance.EndpointAddresses.Contains(_CurrentlySelectedEndpoint) && !string.IsNullOrEmpty(_CurrentlySelectedEndpoint))
                    {
                        Config.Instance.EndpointAddresses.Insert(0, _CurrentlySelectedEndpoint);
                        EndpointHistory.Insert(0, _CurrentlySelectedEndpoint);
                    }


                });
            }
        }

    }
}
