using System;
using System.Collections.Generic;

namespace chattingApp.DataAndContext.ModelsForChattingApp
{
    public partial class Group
    {
        public Group()
        {
            GroupMembers = new HashSet<GroupMember>();
        }

        public int Id { get; set; }
        public string GroupName { get; set; } = null!;
        public string CreatedById { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<GroupMember> GroupMembers { get; set; }
    }
}
