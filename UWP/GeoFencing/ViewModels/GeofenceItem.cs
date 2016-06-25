using System;
using Windows.Devices.Geolocation;
using Windows.Devices.Geolocation.Geofencing;

namespace GeoFencing.ViewModels
{
    // GeofenceItem implements IEquatable to allow
    // removal of objects in the collection
    public class GeofenceItem : IEquatable<GeofenceItem>
    {
        private Geofence _geofence;
        private string _id;

        public GeofenceItem(Geofence geofence)
        {
            this._geofence = geofence;
            this._id = geofence.Id;
        }

        public bool Equals(GeofenceItem other)
        {
            bool isEqual = false;
            if (Id == other.Id)
            {
                isEqual = true;
            }

            return isEqual;
        }

        public Geofence Geofence
        {
            get
            {
                return _geofence;
            }
        }

        public string Id
        {
            get
            {
                return _id;
            }
        }

        public BasicGeoposition Location
        {
            get
            {
                return new BasicGeoposition()
                {
                    Latitude = this.Latitude,
                    Longitude = this.Longitude
                };
            }
        }

        public double Latitude
        {
            get
            {
                Geocircle circle = _geofence.Geoshape as Geocircle;
                return circle.Center.Latitude;
            }
        }

        public double Longitude
        {
            get
            {
                Geocircle circle = _geofence.Geoshape as Geocircle;
                return circle.Center.Longitude;
            }
        }

        public double Radius
        {
            get
            {
                Geocircle circle = _geofence.Geoshape as Geocircle;
                return circle.Radius;
            }
        }

        public bool SingleUse
        {
            get
            {
                return _geofence.SingleUse;
            }
        }

        public MonitoredGeofenceStates MonitoredStates
        {
            get
            {
                return _geofence.MonitoredStates;
            }
        }

        public TimeSpan DwellTime
        {
            get
            {
                return _geofence.DwellTime;
            }
        }

        public DateTimeOffset StartTime
        {
            get
            {
                return _geofence.StartTime;
            }
        }

        public TimeSpan Duration
        {
            get
            {
                return _geofence.Duration;
            }
        }
    }
}