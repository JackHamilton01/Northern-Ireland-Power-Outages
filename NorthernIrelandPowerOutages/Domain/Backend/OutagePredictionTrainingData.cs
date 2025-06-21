using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Backend
{
    /// <summary>
    /// Used for migrations for outage prediction API
    /// </summary>
    public class OutagePredictionTrainingData
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public required virtual int Id { get; set; }
        public required virtual float Temp { get; set; }
        public required virtual float WindSpeed { get; set; }
        public required virtual float Rain { get; set; }
        public required virtual int Thunderstorm { get; set; }
        public required virtual int Outage { get; set; }
    }
}
