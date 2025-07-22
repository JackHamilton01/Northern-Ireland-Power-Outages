using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Backend
{
    public class OutagePredictionTrainingData
    {
        [JsonPropertyName("lat")]
        public virtual double Latitude { get; set; }
        [JsonPropertyName("lon")]
        public virtual double Longitude { get; set; }
        [JsonPropertyName("outage")]
        public virtual int Outage { get; set; }

        // Used for entity
        public virtual int Id { get; set; }
        public virtual float Temp { get; set; }
        public virtual float WindSpeed { get; set; }
        public virtual float Rain { get; set; }
        public virtual int Thunderstorm { get; set; }
    }
}
