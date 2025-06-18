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
        public string FileName { get; set; }

        public static implicit operator Hazard(HazardUI hazard)
        {
            return new Hazard
            {
                Id = hazard.Id,
                Title = hazard.Title,
                Description = hazard.Description,
                FileName = hazard.FileName
            };
        }
    }
}
