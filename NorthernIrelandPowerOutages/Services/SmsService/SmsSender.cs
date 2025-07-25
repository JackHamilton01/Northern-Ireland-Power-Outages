using Infrastructure.ProjectSettings;
using Infrastructure.Sms;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace SmsService
{
    public class SmsSender : ISmsSender
    {
        private readonly TwilioSettings settings;

        public SmsSender(IOptions<TwilioSettings> options)
        {
            settings = options.Value;

            TwilioClient.Init(settings.TwilioAccountSid, settings.TwilioAuthToken);
        }

        public async Task<SmsResult> SendMessageAsync(string targetPhoneNumber, string messageContent)
        {
            return await SendMessage(settings.TwilioPhoneNumber, targetPhoneNumber, messageContent);
        }

        private async Task<SmsResult> SendMessage(string sourcePhoneNumber, string targetPhoneNumber, string messageContent)
        {
            MessageResource? message = await MessageResource.CreateAsync(
                        to: new PhoneNumber(targetPhoneNumber),
                        from: new PhoneNumber(sourcePhoneNumber),
                        body: messageContent);

            return new SmsResult
            {
                IsSuccessful = message.Status != MessageResource.StatusEnum.Failed,
                MessageSid = message.Sid,
                ErrorMessage = message.ErrorMessage
            };
        }
    }
}
