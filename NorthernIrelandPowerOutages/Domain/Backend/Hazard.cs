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
        public virtual required int Id { get; set; }
        public virtual required string Title { get; set; }
        public virtual required string Description { get; set; }

        [MaxLength(2000)]
        public virtual required string FileName { get; set; }

        public static implicit operator HazardUI(Hazard hazard)
        {
            return new HazardUI
            {
                Id = hazard.Id,
                Title = hazard.Title,
                Description = hazard.Description,
                FileName = hazard.FileName
            };
        }
    }
}
