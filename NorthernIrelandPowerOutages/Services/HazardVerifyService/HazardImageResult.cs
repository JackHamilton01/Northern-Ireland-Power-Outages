using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HazardVerifyService
{
    public class HazardImageResult
    {
        public bool IsMatchWithTitle { get; }
        public string Response { get; }

        public HazardImageResult(bool isMatchWithTitle, string response)
        {
            IsMatchWithTitle = isMatchWithTitle;
            Response = response;
        }
    }
}
