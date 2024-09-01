using System.ComponentModel.DataAnnotations;

namespace chattingApp.vModels
{
    public class VerificationOtp
    {
        public string phoneNumber { get; set; }
        public string OTP { get; set;}
    }
}
