using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Backend
{
    public class HazardImage
    {
        public virtual int Id { get; set; }
        public virtual required string FileName { get; set; }
    }
}
