using System.ComponentModel.DataAnnotations;

namespace chattingApp.vModels
{
    public class phoneOtpModel
    {
        [Required]
        [RegularExpression(@"^(010|011|012|015)[0-9]{8}$", ErrorMessage = "Invalid Egyptian phone number.")]
        public string PhoneNumber { get; set; }
        [Required]
        public string otp { get; set; }
        [Required]
        public DateTime validTo { get; set; }
    }
}
