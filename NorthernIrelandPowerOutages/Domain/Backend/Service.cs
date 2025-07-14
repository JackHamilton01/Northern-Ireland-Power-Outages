using Domain.Frontend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Backend
{
    public class Service
    {
        public virtual int  Id { get; set; }
        public required virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual double Latitude { get; set; }
        public virtual double Longitude { get; set; }

        public static implicit operator ServiceUI(Service service)
        {
            return new ServiceUI
            {
                Name = service.Name,
                Description = service.Description,
                Latitude = service.Latitude,
                Longitude = service.Longitude
            };
        }
    }
}
