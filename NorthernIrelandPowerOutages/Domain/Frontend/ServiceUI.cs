using Domain.Backend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Frontend
{
    public class ServiceUI
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public static implicit operator Service(ServiceUI service)
        {
            return new Service
            {
                Name = service.Name,
                Description = service.Description,
                Latitude = service.Latitude,
                Longitude = service.Longitude
            };
        }
    }
}
