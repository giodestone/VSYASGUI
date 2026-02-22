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
                Config.Instance.CurrentApiKey = value;
            }
        }

        public string CurrentlySelectedEndpoint
        {
            get => _CurrentlySelectedEndpoint;
            set
            {
                Update(ref _CurrentlySelectedEndpoint, value);
                Config.Instance.CurrentEndpoint = value;
            }
        }

        public ConfigPresenter()
        {
            InitObservableCollections();

            if (Config.Instance.ApiKeyHistory.Count > 0)
                Config.Instance.CurrentApiKey = Config.Instance.ApiKeyHistory[0];

            if (Config.Instance.EndpointAddresses.Count > 0)
                Config.Instance.CurrentEndpoint = Config.Instance.EndpointAddresses[0];
        }

        private void InitObservableCollections()
        {
            _ApiKeyHistory = new ObservableCollection<string>(Config.Instance.ApiKeyHistory);
            _EndpointHistory = new ObservableCollection<string>(Config.Instance.EndpointAddresses);
        }

        /// <summary>
        /// Adds the currently selected ApiKey/Endpoint to their respective collections, if not already present. Basically provides a history of entries.
        /// </summary>
        public ICommand AddCurrentlySelectedToCollection
        {
            get
            {
                return new Command(_ =>
                {
                    if (!Config.Instance.ApiKeyHistory.Contains(Config.Instance.CurrentApiKey) && !string.IsNullOrEmpty(Config.Instance.CurrentApiKey))
                    {
                        Config.Instance.ApiKeyHistory.Insert(0, Config.Instance.CurrentApiKey);
                        ApiKeyHistory.Insert(0, Config.Instance.CurrentApiKey);
                    }

                    if (!Config.Instance.EndpointAddresses.Contains(Config.Instance.CurrentEndpoint) && !string.IsNullOrEmpty(Config.Instance.CurrentEndpoint))
                    {
                        Config.Instance.EndpointAddresses.Insert(0, Config.Instance.CurrentEndpoint);
                        EndpointHistory.Insert(0, Config.Instance.CurrentEndpoint);
                    }
                });
            }
        }
    }
}
