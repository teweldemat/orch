using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace orch.core.ef
{
    public sealed partial class CoreEFDbFunctions
    {
        private static readonly Lazy<CoreEFDbFunctions> _instance = new(() => new CoreEFDbFunctions());

        // Private constructor ensures that no instances can be created from outside this class
        private CoreEFDbFunctions() { }

        // Public static method to provide access to the single instance of this class
        public static CoreEFDbFunctions Instance => _instance.Value;

        [DbFunction(Schema = "public", Name = "ST_ClusterDBSCAN")]
        public int? ClusterDBSCAN(Geometry geometry, double eps, int minpoints)
        {
            throw new NotSupportedException("This method should only be used in LINQ expressions. It has no in-memory implementation.");
        }

        [DbFunction(Schema = "public", Name = "ST_ClusterKMeans")]
        public int? ClusterKMeans(Geometry geometry, int numClusters)
        {
            throw new NotSupportedException("This method should only be used in LINQ expressions. It has no in-memory implementation.");
        }

        public static double ComputeEps(double zoomLevel)
        {
            // Scale eps inversely with zoom level: higher zoom level -> smaller eps
            double maxZoomLevel = 18;
            double minEps = 0.0001;
            double maxEps = 0.01;

            return minEps + (maxEps - minEps) * (maxZoomLevel - zoomLevel) / maxZoomLevel;
        }

        public static int ComputeMinPoints(double zoomLevel)
        {
            // Scale minPoints inversely with zoom level: higher zoom level -> smaller minPoints
            double maxZoomLevel = 18;
            int minMinPoints = 2;
            int maxMinPoints = 10;

            return (int)(minMinPoints + (maxMinPoints - minMinPoints) * (maxZoomLevel - zoomLevel) / maxZoomLevel);
        }
    }
}
