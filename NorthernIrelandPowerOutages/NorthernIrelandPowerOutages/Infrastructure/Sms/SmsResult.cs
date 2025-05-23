using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Sms
{
    public class SmsResult
    {
        public required bool IsSuccessful { get; set; }
        public required string MessageSid { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
