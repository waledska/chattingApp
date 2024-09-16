namespace chattingApp.vModels
{
    public class ContactWithMessage
    {
        // contact data
        public string contactId { get; set; }
        public string contactName { get; set; }
        public string imgUrlForContact { get; set; }

        // last message data
        public bool isGroup { get; set; }
        public string senderNameForMessageInGroup { get; set; }
        public string messageText { get; set; }
        public DateTime timeOfMessage { get; set; }
        public string statusOfMessage { get; set; }
        public bool isMessageDeleted { get; set; }
        public string senderName { get; set;}
    } 
}
