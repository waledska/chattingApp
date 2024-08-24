using Microsoft.AspNetCore.Identity;

namespace chattingApp.DataAndContext.Models
{
    public class ApplicationUser: IdentityUser
    {
        // modify in the properties here saksoook!
        public bool phoneNumIsVerified { get; set; }
        public DateTime lastOnlineTime { get; set; }
    }
}
