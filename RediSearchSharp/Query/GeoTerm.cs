using System;
using System.Globalization;
using StackExchange.Redis;

namespace RediSearchSharp.Query
{
    /// <summary>
    /// Encapsulates a geospatial search term.
    /// </summary>
    public struct GeoTerm
    {
        public double Longitude { get; }
        public double Latitude { get; }
        public double Distance { get; }
        public GeoUnit Unit { get; }

        private GeoTerm(double longitude, double latitude, double distance, GeoUnit unit)
        {
            Longitude = longitude;
            Latitude = latitude;
            Distance = distance;
            Unit = unit;
        }

        /// <summary>
        /// Creates a geospatial search term.
        /// </summary>
        /// <param name="longitude">The longitude value of the center geographic coordinate. Valid longitudes are from -180 to 180 degrees.</param>
        /// <param name="latitude">The latitude value of the center geographic coordinate. Valid latitudes are from -85.05112878 to 85.05112878 degrees.</param>
        /// <param name="distance">The maximum distance measured from the center (radius).</param>
        /// <param name="unit">The radius unit.</param>
        /// <returns>A geospatial search term matching the specified area.</returns>
        public static GeoTerm WithinDistanceOf(double longitude, double latitude, double distance, GeoUnit unit)
        {
            return new GeoTerm(longitude, latitude, distance, unit);
        }

        internal string GetValue()
        {
            string unit;
            switch (Unit)
            {
                case GeoUnit.Feet:
                    unit = "ft";
                    break;
                case GeoUnit.Kilometers:
                    unit = "km";
                    break;
                case GeoUnit.Meters:
                    unit = "m";
                    break;
                case GeoUnit.Miles:
                    unit = "mi";
                    break;
                default:
                    throw new ArgumentException();
            }

            return string.Format("[{0} {1} {2} {3}]",
                Longitude.ToString("G17", CultureInfo.InvariantCulture),
                Latitude.ToString("G17", CultureInfo.InvariantCulture),
                Distance.ToString("G17", CultureInfo.InvariantCulture),
                unit
            );
        }
    }
}