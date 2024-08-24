using System;
using System.Collections.Generic;

namespace chattingApp.DataAndContext.ModelsForChattingApp
{
    public partial class Message
    {
        public int Id { get; set; }
        public string MessageText { get; set; } = null!;
        public string SenderId { get; set; } = null!;
        public string? RecieverId { get; set; }
        public int? GroupId { get; set; }
        public DateTime TimeOfSend { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsModified { get; set; }
        public string MessageStatus { get; set; } = null!;
    }
}
