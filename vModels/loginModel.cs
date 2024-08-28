using System.ComponentModel.DataAnnotations;

namespace chattingApp.vModels
{
    public class loginModel
    {
        [Required]
        public string Token { get; set; }
        [Required]
        public string UserName { get; set; }
    }
}
