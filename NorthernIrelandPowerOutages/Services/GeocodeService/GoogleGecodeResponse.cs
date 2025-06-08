using System.Collections.Generic;
using System.Text.Json.Serialization;

public class GoogleGeocodeResponse
{
    [JsonPropertyName("results")]
    public List<Result>? Results { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

public class Result
{
    [JsonPropertyName("address_components")]
    public List<AddressComponent>? AddressComponents { get; set; }

    [JsonPropertyName("formatted_address")]
    public string? FormattedAddress { get; set; }

    [JsonPropertyName("geometry")]
    public Geometry? Geometry { get; set; }

    [JsonPropertyName("navigation_points")]
    public List<NavigationPoint>? NavigationPoints { get; set; }

    [JsonPropertyName("place_id")]
    public string? PlaceId { get; set; }

    [JsonPropertyName("types")]
    public List<string>? Types { get; set; }
}

public class AddressComponent
{
    [JsonPropertyName("long_name")]
    public string? LongName { get; set; }

    [JsonPropertyName("short_name")]
    public string? ShortName { get; set; }

    [JsonPropertyName("types")]
    public List<string>? Types { get; set; }
}

public class Geometry
{
    [JsonPropertyName("bounds")]
    public Bounds? Bounds { get; set; }

    [JsonPropertyName("location")]
    public Location? Location { get; set; }

    [JsonPropertyName("location_type")]
    public string? LocationType { get; set; }

    [JsonPropertyName("viewport")]
    public Bounds? Viewport { get; set; }
}

public class Bounds
{
    [JsonPropertyName("northeast")]
    public Location? Northeast { get; set; }

    [JsonPropertyName("southwest")]
    public Location? Southwest { get; set; }
}

public class Location
{
    [JsonPropertyName("lat")]
    public double Latitude { get; set; }

    [JsonPropertyName("lng")]
    public double Longitude { get; set; }
}

public class NavigationPoint
{
    [JsonPropertyName("location")]
    public NavLocation? Location { get; set; }
}

public class NavLocation
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }
}
