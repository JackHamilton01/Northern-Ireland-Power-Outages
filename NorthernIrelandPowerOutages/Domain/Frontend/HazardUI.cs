using Domain.Backend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Frontend
{
    public class HazardUI
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<HazardImage> Images { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public static implicit operator Hazard(HazardUI hazard)
        {
            return new Hazard
            {
                Id = hazard.Id,
                Title = hazard.Title,
                Description = hazard.Description,
                FileNames = hazard.Images,
                Latitude = hazard.Latitude,
                Longitude = hazard.Longitude
            };
        }
    }
}
