using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HazardVerifyService
{
    public class HazardImageResult
    {
        public string Response { get; }

        public HazardImageResult(string response)
        {
            Response = response;
        }
    }
}
