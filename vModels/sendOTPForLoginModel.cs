using System.ComponentModel.DataAnnotations;

namespace chattingApp.vModels
{
    public class sendOTPForLoginModel
    {
        [Required]
        [RegularExpression(@"^(010|011|012|015)[0-9]{8}$", ErrorMessage = "Invalid Egyptian phone number.")]
        public string phoneNumber { get; set; }
        [Required]
        public string userName { get; set; }
    }
}
