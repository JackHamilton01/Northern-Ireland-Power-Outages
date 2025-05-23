using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.ProjectSettings
{
    public class TwilioSettings
    {
        public required string TwilioPhoneNumber { get; set; }
        public required string TwilioAccountSid { get; set; }
        public required string TwilioAuthToken { get; set; }
    }
}
