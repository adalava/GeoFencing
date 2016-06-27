using BackgroundTask;
using Prism.Commands;
using Prism.Windows.AppModel;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Background;
using Windows.Data.Json;
using Windows.Devices.Geolocation;
using Windows.Devices.Geolocation.Geofencing;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace GeoFencing.ViewModels
{
    public class GeoFencingHistoryPageViewModel : ViewModelBase
    {
        private INavigationService navigationService;
        private ObservableCollection<string> backgroundGeofenceEventHistory = new ObservableCollection<string>();
        private IResourceLoader resourceLoader;

        public GeoFencingHistoryPageViewModel(INavigationService navigationService, IResourceLoader resourceLoader)
        {
            this.navigationService = navigationService;
            this.resourceLoader = resourceLoader;

            this.ClearEventsCommand = new DelegateCommand(ClearEvents);

            this.BackgroundGeofenceEventHistory.CollectionChanged += (sender, args) =>
            {
                this.OnPropertyChanged("TotalEvents");                
            };
        }

        public ObservableCollection<string> BackgroundGeofenceEventHistory
        {
            get
            {
                return this.backgroundGeofenceEventHistory;
            }
            private set
            {
                this.SetProperty(ref this.backgroundGeofenceEventHistory, value);                
            }
        }

        public string TotalEvents
        {
            get
            {
                string totalEventsFormat = this.resourceLoader.GetString("TotalEvents");

                if (string.IsNullOrEmpty(totalEventsFormat))
                {
                    return this.BackgroundGeofenceEventHistory.Count.ToString();
                }
                else
                {
                    return string.Format(totalEventsFormat, this.BackgroundGeofenceEventHistory.Count);
                }
            }
        }

        public ICommand ClearEventsCommand { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            var settings = ApplicationData.Current.LocalSettings;

            var eventsCollection = (string)settings.Values["BackgroundGeofenceEventCollection"];
            if (!string.IsNullOrEmpty(eventsCollection))
            {
                var events = JsonValue.Parse(eventsCollection).GetArray();

                foreach (var eventItem in events)
                {
                    this.BackgroundGeofenceEventHistory.Add(eventItem.GetString());
                }
            }

            base.OnNavigatedTo(e, viewModelState);
        }    
        
        private void ClearEvents()
        {
            var settings = ApplicationData.Current.LocalSettings;
            settings.Values["BackgroundGeofenceEventCollection"] = string.Empty;
            this.BackgroundGeofenceEventHistory.Clear();
        }
    }
}
