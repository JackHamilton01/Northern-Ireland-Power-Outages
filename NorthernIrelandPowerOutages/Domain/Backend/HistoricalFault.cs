using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Backend
{
    public class HistoricalFault
    {
        public required virtual int Id { get; set; }
        public virtual string? StreetNumber { get; set; }
        public required virtual string StreetName { get; set; }
        public required virtual string PostCode { get; set; }
    }
}
