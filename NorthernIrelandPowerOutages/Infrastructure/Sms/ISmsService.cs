using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Sms
{
    public interface ISmsSender
    {
        Task<SmsResult> SendMessageAsync(string targetPhoneNumber, string messageContent);
    }
}
