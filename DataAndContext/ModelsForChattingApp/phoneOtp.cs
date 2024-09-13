using System.ComponentModel.DataAnnotations;

namespace chattingApp.DataAndContext.ModelsForChattingApp
{
    public class phoneOtp
    {
        public int Id { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string otp { get; set; }
        [Required]
        public DateTime validTo { get; set; }
    }
}
