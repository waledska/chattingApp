using System.ComponentModel.DataAnnotations;

namespace chattingApp.vModels
{
    public class newGroupModel
    {
        [Required]
        public string groupName { get; set; }
        [Required]
        public string createdById { get; set; }
        public IFormFile img { get; set; }
    }
}
