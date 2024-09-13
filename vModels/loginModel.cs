using System.ComponentModel.DataAnnotations;

namespace chattingApp.vModels
{
    public class loginModel
    {
        [Required]
        public string Otp { get; set; }
        [Required]
        public string UserName { get; set; }
    }
}
