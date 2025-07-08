using Domain.Frontend;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Backend
{
    public class Hazard
    {
        public virtual int Id { get; set; }
        public virtual required string Title { get; set; }
        public virtual string? Description { get; set; }

        //[MaxLength(2000)]
        public virtual required List<HazardImage> FileNames { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public static implicit operator HazardUI(Hazard hazard)
        {
            return new HazardUI
            {
                Id = hazard.Id,
                Title = hazard.Title,
                Description = hazard.Description,
                Images = hazard.FileNames,
                Latitude = hazard.Latitude,
                Longitude = hazard.Longitude
            };
        }
    }
}
