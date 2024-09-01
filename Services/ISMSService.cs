using Twilio.Rest.Api.V2010.Account;

namespace chattingApp.Services
{
    public interface ISMSService
    {
        MessageResource Send(string phoneNumber, string message);
    }
}
