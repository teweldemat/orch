using NetTopologySuite.Geometries;

namespace orch.common
{
    /// <summary>
    /// Represents a geographical coordinate with a latitude and longitude.
    /// </summary>
    public struct Coordinate
    {
        /// <summary>
        /// The latitude in degrees.
        /// </summary>
        public double Latitude;

        /// <summary>
        /// The longitude in degrees.
        /// </summary>
        public double Longitude;
        public Point Location;
    }

    /// <summary>
    /// Provides methods for calculating distances between geographical coordinates.
    /// </summary>
    public static class DistanceCalculator
    {
        /// <summary>
        /// Helper method to convert angles from degrees to radians.
        /// </summary>
        /// <param name="angle">The angle in degrees.</param>
        /// <returns>The angle in radians.</returns>
        private static double ToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        /// <summary>
        /// Calculates the distance between two coordinates using the Haversine formula.
        /// </summary>
        /// <param name="start">The starting coordinate.</param>
        /// <param name="end">The ending coordinate.</param>
        /// <returns>The distance in meters.</returns>
        public static double CalculateDistanceHaversine(Coordinate start, Coordinate end)
        {
            int R = 6371000; // Radius of the earth in m
            double dLat = ToRadians(end.Latitude - start.Latitude);
            double dLon = ToRadians(end.Longitude - start.Longitude);
            double a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(start.Latitude)) * Math.Cos(ToRadians(end.Latitude)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double distance = R * c; // Distance in m
            return distance;
        }

        /// <summary>
        /// Calculates the distance between two coordinates using the Vincenty formula.
        /// </summary>
        /// <param name="coord1">The first coordinate.</param>
        /// <param name="coord2">The second coordinate.</param>
        /// <returns>The distance in meters.</returns>
        public static double CalculateDistanceVincenty(Coordinate coord1, Coordinate coord2)
        {
            double lat1 = ToRadians(coord1.Latitude);
            double lon1 = ToRadians(coord1.Longitude);
            double lat2 = ToRadians(coord2.Latitude);
            double lon2 = ToRadians(coord2.Longitude);

            double a = 6378137.0, b = 6356752.314245, f = 1 / 298.257223563; // WGS-84 ellipsoid params
            double L = lon2 - lon1;
            double U1 = Math.Atan((1 - f) * Math.Tan(lat1));
            double U2 = Math.Atan((1 - f) * Math.Tan(lat2));
            double sinU1 = Math.Sin(U1), cosU1 = Math.Cos(U1);
            double sinU2 = Math.Sin(U2), cosU2 = Math.Cos(U2);

            double cosSqAlpha;
            double sinSigma;
            double cos2SigmaM;
            double cosSigma;
            double sigma;

            double lambda = L, lambdaP, iterLimit = 100;
            do
            {
                double sinLambda = Math.Sin(lambda), cosLambda = Math.Cos(lambda);
                sinSigma = Math.Sqrt((cosU2 * sinLambda) * (cosU2 * sinLambda) +
                                     (cosU1 * sinU2 - sinU1 * cosU2 * cosLambda) *
                                     (cosU1 * sinU2 - sinU1 * cosU2 * cosLambda));
                if (sinSigma == 0)
                    return 0;  // co-incident points

                cosSigma = sinU1 * sinU2 + cosU1 * cosU2 * cosLambda;
                sigma = Math.Atan2(sinSigma, cosSigma);
                double sinAlpha = cosU1 * cosU2 * sinLambda / sinSigma;
                cosSqAlpha = 1 - sinAlpha * sinAlpha;
                cos2SigmaM = cosSigma - 2 * sinU1 * sinU2 / cosSqAlpha;

                if (double.IsNaN(cos2SigmaM))
                    cos2SigmaM = 0;  // equatorial line

                double C = f / 16 * cosSqAlpha * (4 + f * (4 - 3 * cosSqAlpha));
                lambdaP = lambda;
                lambda = L + (1 - C) * f * sinAlpha *
                         (sigma + C * sinSigma * (cos2SigmaM + C * cosSigma * (-1 + 2 * cos2SigmaM * cos2SigmaM)));
            } while (Math.Abs(lambda - lambdaP) > 1e-12 && --iterLimit > 0);

            if (iterLimit == 0)
                return double.NaN;  // formula failed to converge

            double uSq = cosSqAlpha * (a * a - b * b) / (b * b);
            double A = 1 + uSq / 16384 * (4096 + uSq * (-768 + uSq * (320 - 175 * uSq)));
            double B = uSq / 1024 * (256 + uSq * (-128 + uSq * (74 - 47 * uSq)));
            double deltaSigma = B * sinSigma * (cos2SigmaM + B / 4 *
                                    (cosSigma * (-1 + 2 * cos2SigmaM * cos2SigmaM) -
                                    B / 6 * cos2SigmaM * (-3 + 4 * sinSigma * sinSigma) *
                                    (-3 + 4 * cos2SigmaM * cos2SigmaM)));

            double distance = b * A * (sigma - deltaSigma);

            return distance;
        }
    }
}