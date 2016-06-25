using BackgroundTask;
using Prism.Commands;
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
using Windows.ApplicationModel.Background;
using Windows.Devices.Geolocation;
using Windows.Devices.Geolocation.Geofencing;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace GeoFencing.ViewModels
{
    public class GeoFencingSettingsPageViewModel : ViewModelBase
    {
        private INavigationService navigationService;
        
        private IBackgroundTaskRegistration _geofenceTask = null;
        private bool isTaskRegistered;

        public GeoFencingSettingsPageViewModel(INavigationService navigationService, IResourceLoader resourceLoader)
        {
            this.navigationService = navigationService;
            this.RegisterBackgroundTask = new DelegateCommand(Register);
            this.UnregisterBackgroundTask = new DelegateCommand(Unregister);
        }

        public bool IsTaskRegistered
        {
            get
            {
                return this.isTaskRegistered;
            }
            private set
            {
                this.SetProperty(ref this.isTaskRegistered, value);
            }
        }

        public ICommand RegisterBackgroundTask { get; private set; }

        public ICommand UnregisterBackgroundTask { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            this.IsTaskRegistered = false;

            // Loop through all background tasks to see if SampleGeofenceBackgroundTask is already registered
            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                if (cur.Value.Name == nameof(GeofenceBackgroundTask))
                {
                    _geofenceTask = cur.Value;
                    this.IsTaskRegistered = true;
                    break;
                }
            }

            base.OnNavigatedTo(e, viewModelState);
        }

        private async void Register()
        {
            try
            {
                // Get permission for a background task from the user. If the user has already answered once,
                // this does nothing and the user must manually update their preference via PC Settings.
                BackgroundAccessStatus backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();

                switch (backgroundAccessStatus)
                {

                    case BackgroundAccessStatus.Unspecified:
                    case BackgroundAccessStatus.Denied:
                        Debug.WriteLine("Not able to run in background. Application must be added to the lock screen.");
                        this.IsTaskRegistered = false;
                        break;

                    default:
                        // BckgroundTask is allowed
                        Debug.WriteLine("Geofence background task registered.");
                        
                        // Need to request access to location
                        // This must be done with the background task registration
                        // because the background task cannot display UI.
                        bool locationAccess = await RequestLocationAccess();
                        if (locationAccess)
                        {
                            BackgroundTaskBuilder geofenceTaskBuilder = new BackgroundTaskBuilder();

                            geofenceTaskBuilder.Name = nameof(GeofenceBackgroundTask);
                            geofenceTaskBuilder.TaskEntryPoint = typeof(GeofenceBackgroundTask).ToString();

                            // Create a new location trigger
                            var trigger = new LocationTrigger(LocationTriggerType.Geofence);

                            // Associate the locationi trigger with the background task builder
                            geofenceTaskBuilder.SetTrigger(trigger);
                            geofenceTaskBuilder.AddCondition(new SystemCondition(SystemConditionType.BackgroundWorkCostNotHigh));

                            // Register the background task
                            _geofenceTask = geofenceTaskBuilder.Register();

                            this.IsTaskRegistered = true;
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());                
            }

            if (this.IsTaskRegistered)
            {
                var successDialog = new MessageDialog("Task registered");
                await successDialog.ShowAsync();
            }
            else
            {
                var errorDialog = new MessageDialog("Error registering task");
                await errorDialog.ShowAsync();
            }
        }

        /// <summary>
        /// This is the click handler for the 'Unregister' button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Unregister()
        {
            // Unregister the background task
            if (null != _geofenceTask)
            {
                _geofenceTask.Unregister(true);
                _geofenceTask = null;
            }

            var successDialog = new MessageDialog("Geofence background task unregistered");
            await successDialog.ShowAsync();

            this.IsTaskRegistered = false;
        }

        /// <summary>
        /// Get permission for location from the user. If the user has already answered once,
        /// this does nothing and the user must manually update their preference via Settings.
        /// </summary>
        private async Task<bool> RequestLocationAccess()
        {
            // Request permission to access location
            var accessStatus = await Geolocator.RequestAccessAsync();

            bool result = false;

            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    result = true;
                    break;

                case GeolocationAccessStatus.Denied:
                    Debug.WriteLine("Access to location is denied.");
                    break;

                case GeolocationAccessStatus.Unspecified:
                    Debug.WriteLine("Unspecificed error!");
                    break;
            }

            return result;
        }        
    }
}
