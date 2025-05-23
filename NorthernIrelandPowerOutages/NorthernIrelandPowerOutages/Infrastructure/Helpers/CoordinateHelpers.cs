using DotSpatial.Projections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Helpers
{
    public static class CoordinateHelpers
    {
        private const int IrishGridEpsg = 29903;
        private const int Wgs84Epsg = 4326;

        public static (double Latitude, double Longitude) ConvertIrishGridToLatLon(double easting, double northing)
        {
            double[] xy = { easting, northing };
            double[] z = { 0 }; // Elevation

            ProjectionInfo source = ProjectionInfo.FromEpsgCode(IrishGridEpsg); 
            ProjectionInfo dest = ProjectionInfo.FromEpsgCode(Wgs84Epsg);   

            Reproject.ReprojectPoints(xy, z, source, dest, 0, 1);

            return (xy[1], xy[0]);
        }
    }
}
