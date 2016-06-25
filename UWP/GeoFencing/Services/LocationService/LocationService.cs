using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace GeoFencing.Services.LocationService
{
    public class LocationService
    {
        private static LocationService instance = null;

        private LocationService()
        {
        }

        public static LocationService GetInstance()
        {
            if (instance == null)
            {
                instance = new LocationService();
            }

            return instance;
        }

        private CancellationTokenSource _cts = null;

        public async Task<Geoposition> GetUserLocation(PositionAccuracy accuracy)
        {
            // Get cancellation token
            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;

            // Carry out the operation
            // geolocator and location permissions are initialized and checked on page creation.
            var geolocator = new Geolocator();

            // Request a high accuracy position for better accuracy locating the geofence
            geolocator.DesiredAccuracy = accuracy;

            return await geolocator.GetGeopositionAsync().AsTask(token);
        }
    }
}
