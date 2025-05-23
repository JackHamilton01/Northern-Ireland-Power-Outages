using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.Diagnostics;

namespace NorthernIrelandPowerOutages.Counties
{
    public class CountyMatcher
    {
        private static string geoJsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Map", "NorthernIrelandCounties.geojson");
        private FeatureCollection counties;

        public Dictionary<string, int> CountyOutages { get; } =
            NorthernIrelandCounties.Counties.ToDictionary(county => county,
            _ => 0,
            StringComparer.OrdinalIgnoreCase);

        private CountyMatcher(FeatureCollection counties)
        {
            this.counties = counties;
        }

        public static async Task<CountyMatcher> Create()
        {
            return new CountyMatcher(await GetCountiesFromGeoJson());
        }

        public string MatchCounties(double longitude, double latitude)
        {
            Point faultPoint = new(longitude, latitude) { SRID = 4326 };

            foreach (var feature in counties)
            {
                if (feature.Geometry.Contains(faultPoint))
                {
                    string countyName = (string)feature.Attributes["CountyName"];
                    Debug.WriteLine($"Point is in county: {countyName}");

                    return countyName;
                }
            }

            return string.Empty;
        }

        private static async Task<FeatureCollection> GetCountiesFromGeoJson()
        {
            var geoJsonReader = new GeoJsonReader();

            using (StreamReader reader = new(geoJsonPath))
            {
                string json = await reader.ReadToEndAsync();
                return geoJsonReader.Read<FeatureCollection>(json);
            }
        }
    }
}