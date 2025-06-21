using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class OutagePredictionModel
    {
        public int Id { get; set; }
        public byte[] ModelData { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
