using System.ComponentModel.DataAnnotations;

namespace chattingApp.vModels
{
    public class userUpdateDataModel
    {
        [Required]
        public string name { get; set; }
        [Required]
        [DataType(DataType.Upload)]
        public IFormFile img { get; set; }
    }
}
