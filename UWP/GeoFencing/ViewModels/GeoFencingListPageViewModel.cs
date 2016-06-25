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
using Windows.Devices.Geolocation;
using Windows.Devices.Geolocation.Geofencing;
using Windows.Globalization;
using Windows.Globalization.DateTimeFormatting;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Maps;

namespace GeoFencing.ViewModels
{
    public class GeoFencingListPageViewModel : ViewModelBase
    {
        private INavigationService navigationService;
        private Calendar calendar;
        private DateTimeFormatter formatterLongTime;

        public delegate void OnUpdateLocationDataHandler(object sender, BasicGeoposition pos);
        public event OnUpdateLocationDataHandler OnUpdateLocationData;

        public GeoFencingListPageViewModel(INavigationService navigationService, IResourceLoader resourceLoader)
        {
            this.navigationService = navigationService;

            this.RemoveGeoFencing = new DelegateCommand(RemoveGeoFencingItem);

            this.EditGeoFencing = new DelegateCommand(EditGeoFencingItem);

            this.AddGeoFencing = new DelegateCommand(AddGeoFencingItem);
        }

        public ObservableCollection<GeofenceItem> GeoFencingList { get; set; }
        
        public ICommand RemoveGeoFencing { get; private set; }

        public ICommand AddGeoFencing { get; private set; }
        public GeofenceItem SelectedGeoFencing { get; set; }
        public DelegateCommand EditGeoFencing { get; private set; }

        public override async void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);

            this.calendar = new Calendar();
            this.formatterLongTime = new DateTimeFormatter("{hour.integer}:{minute.integer(2)}:{second.integer(2)}", new[] { "en-US" }, "US", Windows.Globalization.CalendarIdentifiers.Gregorian, Windows.Globalization.ClockIdentifiers.TwentyFourHour);
            this.GeoFencingList = new ObservableCollection<GeofenceItem>();

            await this.InitializeGeolocation();
        }

        private async Task InitializeGeolocation()
        {
            //Get permission to use location
            var accessStatus = await Geolocator.RequestAccessAsync();
            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    var geofences = GeofenceMonitor.Current.Geofences;

                    SetGeofenceList(geofences);

                    // register for state change events
                    GeofenceMonitor.Current.GeofenceStateChanged += Current_GeofenceStateChanged; ;
                    GeofenceMonitor.Current.StatusChanged += Current_StatusChanged; ;

                    // set my current position
                    Geolocator geolocator = new Geolocator { DesiredAccuracyInMeters = 10 };
                    geolocator.PositionChanged += OnPositionChanged;

                    // Carry out the operation.
                    Geoposition pos = await geolocator.GetGeopositionAsync();
                    UpdateLocationData(pos);
                    break;

                case GeolocationAccessStatus.Denied:
                    Debug.WriteLine("Access denied.");
                    UpdateLocationData(null);
                    break;

                case GeolocationAccessStatus.Unspecified:
                    Debug.WriteLine("Unspecified error.");
                    UpdateLocationData(null);
                    break;
            }
        }

        private void OnPositionChanged(Geolocator sender, PositionChangedEventArgs e)
        {            
            UpdateLocationData(e.Position);
        }

        private void UpdateLocationData(Geoposition position)
        {
            OnUpdateLocationData?.Invoke(this, new BasicGeoposition() {
                Latitude = position.Coordinate.Latitude,
                Longitude = position.Coordinate.Longitude
            });
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            GeofenceMonitor.Current.GeofenceStateChanged -= Current_GeofenceStateChanged;
            GeofenceMonitor.Current.StatusChanged -= Current_StatusChanged;

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private void SetGeofenceList(IList<Geofence> geofences)
        {
            foreach (Geofence geofence in geofences)
            {
                this.GeoFencingList.Insert(0, new GeofenceItem(geofence));
            }
        }

        private async void Current_StatusChanged(GeofenceMonitor sender, object args)
        {
            var reports = sender.ReadReports();

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                foreach (GeofenceStateChangeReport report in reports)
                {
                    GeofenceState state = report.NewState;
                    Geofence geofence = report.Geofence;
                    string eventDescription = GetTimeStampedMessage(geofence.Id) +
                                              " (" + state.ToString();

                    Debug.WriteLine(eventDescription);
                }
            });
        }

        private async void Current_GeofenceStateChanged(GeofenceMonitor sender, object args)
        {
            var status = sender.Status;

            string eventDescription = GetTimeStampedMessage("Geofence Status Changed");
            eventDescription += " (" + status.ToString() + ")";

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                var dialog = new MessageDialog(eventDescription);
                await dialog.ShowAsync();
            });
        }

        private void RemoveGeoFencingItem()
        {
            if (this.SelectedGeoFencing != null)
            {
                GeofenceMonitor.Current.Geofences.Remove(this.SelectedGeoFencing.Geofence);
                this.GeoFencingList.Remove(this.SelectedGeoFencing);                
            }
        }

        public void AddGeoFencingItem(Geopoint position)
        {
            this.navigationService.Navigate(PageTokens.GeoFencingItemPage, position);
        }

        private void AddGeoFencingItem()
        {
            this.navigationService.Navigate(PageTokens.GeoFencingItemPage, null);
        }

        private void EditGeoFencingItem()
        {
            this.navigationService.Navigate(PageTokens.GeoFencingItemPage, this.SelectedGeoFencing);
        }

        private string GetTimeStampedMessage(string eventCalled)
        {
            calendar.SetToNow();
            return eventCalled + " " + formatterLongTime.Format(calendar.GetDateTime());
        }
    }
}