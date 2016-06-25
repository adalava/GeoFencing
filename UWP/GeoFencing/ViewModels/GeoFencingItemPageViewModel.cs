using Prism.Commands;
using Prism.Mvvm;
using Prism.Windows.AppModel;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Devices.Geolocation;
using Windows.Devices.Geolocation.Geofencing;
using Windows.UI.Core;
using Windows.UI.Popups;

namespace GeoFencing.ViewModels
{
    public class GeoFencingItemPageViewModel : ViewModelBase
    {
        private INavigationService navigationService;
        private string name;
        private double latitude, longitude, radius;
        private IList<Geofence> geofences;
        private string editingGeofenceName = null;

        public GeoFencingItemPageViewModel(INavigationService navigationService, IResourceLoader resourceLoader)
        {
            this.navigationService = navigationService;
            this.SaveGeofencing = new DelegateCommand(SaveGeofenceItem);
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.SetProperty(ref this.name, value);
            }
        }

        public double Latitude
        {
            get
            {
                return this.latitude;
            }
            set
            {
                this.SetProperty(ref this.latitude, value);
            }
        }

        public double Longitude
        {
            get
            {
                return this.longitude;
            }
            set
            {
                this.SetProperty(ref this.longitude, value);
            }
        }

        public double Radius
        {
            get
            {
                return this.radius;
            }
            set
            {
                this.SetProperty(ref this.radius, value);
            }
        }

        public ICommand SaveGeofencing { get; private set; }

        public override async void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            if (e.Parameter != null)
            {
                if (e.Parameter is GeofenceItem)
                {
                    var geofence = (GeofenceItem)e.Parameter;
                    this.Name = geofence.Id;
                    this.Latitude = geofence.Latitude;
                    this.Longitude = geofence.Longitude;
                    this.Radius = geofence.Radius;

                    this.editingGeofenceName = this.Name;
                }
                else
                {
                    var geopoint = (Geopoint)e.Parameter;
                    this.Latitude = geopoint.Position.Latitude;
                    this.Longitude = geopoint.Position.Longitude;
                }
            }

            // Get permission to use location
            var accessStatus = await Geolocator.RequestAccessAsync();
            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    geofences = GeofenceMonitor.Current.Geofences;
                    break;

                case GeolocationAccessStatus.Denied:
                    Debug.WriteLine("Access denied.");
                    break;

                case GeolocationAccessStatus.Unspecified:
                    Debug.WriteLine("Unspecified error.");
                    break;
            }

            base.OnNavigatedTo(e, viewModelState);
        }

        private async void SaveGeofenceItem()
        {
            bool success = false;

            // Define the fence location and radius.
            BasicGeoposition position;
            position.Latitude = this.Latitude;
            position.Longitude = this.Longitude;
            position.Altitude = 0.0;

            // Set a circular region for the geofence.
            Geocircle geocircle = new Geocircle(position, this.Radius);

            // Set the monitored states.
            MonitoredGeofenceStates monitoredStates =
                            MonitoredGeofenceStates.Entered |
                            MonitoredGeofenceStates.Exited |
                            MonitoredGeofenceStates.Removed;

            // check if there is a geo fence with same name. if yes, remove it
            var currGeofence = geofences.Where(gf => gf.Id == this.editingGeofenceName).FirstOrDefault();
            int index = geofences.Count;

            if (currGeofence != null)
            {
                index = this.geofences.IndexOf(currGeofence);
                this.geofences.Remove(currGeofence);
                this.editingGeofenceName = string.Empty;
            }

            // Create the geofence.
            Geofence geofence = new Geofence(this.Name, geocircle, monitoredStates, false);

            try
            {
                // Add to geo fence monitor
                geofences.Insert(index, geofence);
                success = true;
            }
            catch (Exception ex)
            {
                success = false;                
            }

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                if (success)
                {
                    var dialog = new MessageDialog("Geofence saved!");
                    await dialog.ShowAsync();
                }
                else
                {
                    var dialog = new MessageDialog("Error saving geofence");
                    await dialog.ShowAsync();
                }
            });
        }
    }
}
