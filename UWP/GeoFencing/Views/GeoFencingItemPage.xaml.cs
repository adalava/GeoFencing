using GeoFencing.Services.LocationService;
using GeoFencing.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GeoFencing.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GeoFencingItemPage : Page
    {
        public GeoFencingItemPageViewModel ViewModel { get; private set; }

        public GeoFencingItemPage()
        {
            this.InitializeComponent();

            this.DataContextChanged += GeoFencingItemPage_DataContextChanged;
        }

        private void GeoFencingItemPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            this.ViewModel = (GeoFencingItemPageViewModel)args.NewValue;
        }

        /// <summary>
        /// This is the click handler for the 'Set Lat/Long to current position' button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async private void OnSetPositionToHere(object sender, RoutedEventArgs e)
        {
            SetPositionProgressBar.Visibility = Visibility.Visible;
            SetPositionToHereButton.IsEnabled = false;
            Latitude.IsEnabled = false;
            Longitude.IsEnabled = false;

            try
            {
                Geoposition pos = await LocationService.GetInstance().GetUserLocation(PositionAccuracy.High);

                this.ViewModel.Latitude = pos.Coordinate.Point.Position.Latitude;
                this.ViewModel.Longitude = pos.Coordinate.Point.Position.Longitude;
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("Task canceled");                
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unexpected error");
            }

            SetPositionProgressBar.Visibility = Visibility.Collapsed;
            SetPositionToHereButton.IsEnabled = true;
            Latitude.IsEnabled = true;
            Longitude.IsEnabled = true;
        }
    }
}
