using System.ComponentModel.DataAnnotations;

namespace chattingApp.vModels
{
    public class updateGroupModel
    {
        [Required]
        public int id { get; set; }
        [Required]
        public string groupName { get; set; }
        [Required]
        public string createdById { get; set; }
        public string imgUrl { get; set; }
    }
}
