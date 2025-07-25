using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Requests
{
    public class UpdateAddressAlertsRequest
    {
        public string? UserId { get; set; }
        public int AddressId { get; set; }
        public bool SendSmsAlerts { get; set; }
        public bool SendEmailAlerts { get; set; }
    }
}
