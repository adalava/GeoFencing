using GeoFencing.Services.LocationService;
using GeoFencing.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GeoFencing.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GeoFencingListPage : Page
    {
        private MapIcon mapPosition;

        public GeoFencingListPageViewModel ViewModel { get; private set; }

        public GeoFencingListPage()
        {            
            this.InitializeComponent();
            this.DataContextChanged += GeoFencingListPage_DataContextChanged;

            this.myMap.MapServiceToken = "y4A1ldqUCVz9e0dVRa48~LYNwi-8wgdml-qcx7ciHkg~AstRLIf17UPxXg1KLOHoND80f4CHaHzQoNlhDTuoXgtI8UxbEV02HZiRbSmZVdkL";

            this.myMap.Loaded += MyMap_Loaded;
        }

        private void GeoFencingListPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            this.ViewModel = (GeoFencingListPageViewModel)args.NewValue;
            this.ViewModel.GeoFencingList.CollectionChanged += GeoFencingList_CollectionChanged;
            this.ViewModel.OnUpdateLocationData += ViewModel_OnUpdateLocationData;
        }

        private async void ViewModel_OnUpdateLocationData(object sender, BasicGeoposition pos)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // your position
                if (mapPosition == null)
                {
                    mapPosition = new MapIcon();

                    // Adds the pushpin to the map.
                    myMap.MapElements.Add(mapPosition);
                }

                mapPosition.Location = new Geopoint(pos);

                mapPosition.NormalizedAnchorPoint = new Point(0.5, 1.0);
                mapPosition.Title = "I'm Here";
                mapPosition.ZIndex = 0;
            });
        }

        private void GeoFencingList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.GenerateMapAccuracyCircle();
        }

        private void GenerateMapAccuracyCircle()
        { 
            this.myMap.MapElements.Clear();

            var result = new List<MapPolygon>();

            foreach (GeofenceItem item in this.ViewModel.GeoFencingList)
            {
                double centerLatitude = item.Latitude;
                double centerLongitude = item.Longitude;

                MapPolygon precision = new MapPolygon();
                precision.StrokeThickness = 1;
                precision.FillColor = Windows.UI.Color.FromArgb(80, 255, 0, 0);
            
                var earthRadius = 6371;
                var lat = item.Latitude * Math.PI / 180.0; //radians
                var lon = item.Longitude * Math.PI / 180.0; //radians
                var d = item.Radius / 1000 / earthRadius; // d = angular distance covered on earths surface

                List<BasicGeoposition> precisionPath = new List<BasicGeoposition>();
                for (int x = 0; x <= 360; x++)
                {
                    var brng = x * Math.PI / 180.0; //radians
                    var latRadians = Math.Asin(Math.Sin(lat) * Math.Cos(d) + Math.Cos(lat) * Math.Sin(d) * Math.Cos(brng));
                    var lngRadians = lon + Math.Atan2(Math.Sin(brng) * Math.Sin(d) * Math.Cos(lat), Math.Cos(d) - Math.Sin(lat) * Math.Sin(latRadians));

                    var pt = new BasicGeoposition()
                    {
                        Latitude = 180.0 * (latRadians / Math.PI),
                        Longitude = 180.0 * (lngRadians / Math.PI)
                    };

                    precisionPath.Add(pt);
                }

                precision.Path = new Geopath(precisionPath);                

                this.myMap.MapElements.Add(precision);
            }
        }
        
        private async void MyMap_Loaded(object sender, RoutedEventArgs e)
        {
            var position = await LocationService.GetInstance().GetUserLocation(PositionAccuracy.Default);

            var basicPosition = new BasicGeoposition();
            basicPosition.Latitude = position.Coordinate.Latitude;
            basicPosition.Longitude = position.Coordinate.Longitude;

            this.myMap.Center = new Windows.Devices.Geolocation.Geopoint(basicPosition);

            this.GenerateMapAccuracyCircle();
        }        

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                RemoveGeoFencingAppBarButton.IsEnabled = true;
                EditGeoFencingAppBarButton.IsEnabled = true;
            }
            else
            {
                RemoveGeoFencingAppBarButton.IsEnabled = false;
                EditGeoFencingAppBarButton.IsEnabled = false;
            }
        }

        private void myMap_MapHolding(MapControl sender, MapInputEventArgs args)
        {
            this.ViewModel.AddGeoFencingItem(args.Location);
        }

        private void AlternatingRowListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (ViewModel.GeoFencingList.Count == 0)
            {
                this.GeoFencingListEmpty.Visibility = Visibility.Visible;
            }
            else
            {
                this.GeoFencingListEmpty.Visibility = Visibility.Collapsed;
            }            
        }
    }
}
