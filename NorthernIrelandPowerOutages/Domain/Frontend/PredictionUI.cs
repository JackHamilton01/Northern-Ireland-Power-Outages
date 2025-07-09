using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Frontend
{
    public class PredictionUI
    {
        [JsonPropertyName("lat")]
        public double Latitude { get; init; }  
        [JsonPropertyName("lon")]
        public double Longitude { get; init; }  
        [JsonPropertyName("probability")]
        public double Probability { get; init; }  
    }
}
