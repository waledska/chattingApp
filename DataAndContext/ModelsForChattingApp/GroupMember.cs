using System;
using System.Collections.Generic;

namespace chattingApp.DataAndContext.ModelsForChattingApp
{
    public partial class GroupMember
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public int GroupId { get; set; }
        public DateTime UserJoinedAt { get; set; }
        public DateTime? UserRemovedAt { get; set; }

        public virtual Group Group { get; set; } = null!;
    }
}
