namespace GeocodeService
{
    public partial class GeocodeService
    {
        internal class Result
        {
            public string Formatted_Address { get; set; }
            public Geometry Geometry { get; set; }

        }
    }
}
