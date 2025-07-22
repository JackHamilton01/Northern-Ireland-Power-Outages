using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Backend
{
    public class Settings
    {
        public virtual required int Id { get; set; }
        public virtual required string Name { get; set; }
        public virtual required string Value { get; set; }
    }
}
