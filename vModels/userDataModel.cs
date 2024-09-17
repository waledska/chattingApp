using System.ComponentModel.DataAnnotations;

namespace chattingApp.vModels
{
    public class userDataModel
    {
        [Required]
        public string name { get; set; }
        [Required]
        [RegularExpression(@"^(010|011|012|015)[0-9]{8}$", ErrorMessage = "Invalid Egyptian phone number.")]
        public string phoneNumber { get; set; }
        [Required]
        public string OTPforPhoneConfirmaiton { get; set; }
        [Required]
        [DataType(DataType.Upload)]
        public IFormFile img { get; set; }

        public string message { get; set; }
    }
}
