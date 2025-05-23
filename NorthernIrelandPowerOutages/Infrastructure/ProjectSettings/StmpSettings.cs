using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.ProjectSettings
{
    public class SmtpSettings
    {
        public required string Server { get; set; }
        public required int Port { get; set; }
        public required string SourceEmail { get; set; }
        public required string SourceEmailPassword { get; set; }
    }
}
