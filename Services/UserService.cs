using chattingApp.DataAndContext;
using chattingApp.vModels;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;

namespace chattingApp.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;

        public UserService(ApplicationDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        // for the main page [get contacts for user ordered by last massages(you will know the contacts for the user by messages between sender and reciever)]
        public async Task<List<ContactWithMessage>> getContactsForUserAsync()
        {
            var userId = _authService.getUserId();

            // Fetch groups with the last message data
            var groupsWithLastMessages = await _context.GroupMembers
                .AsNoTracking()
                .Where(gm => gm.UserId == userId)
                .Include(gm => gm.Group)
                .Select(gm => new
                {
                    ContactId = gm.Group.Id.ToString(),
                    ContactName = gm.Group.GroupName,
                    ImgUrl = gm.Group.imgUrl,
                    IsGroup = true,
                    LastMessage = _context.Messages
                        .Where(m => m.GroupId == gm.Group.Id)
                        .OrderByDescending(m => m.TimeOfSend)
                        .FirstOrDefault(),
                })
                .ToListAsync();

            // Convert groups to ContactWithMessage objects outside the query to handle null checks
            var groupContacts = groupsWithLastMessages.Select(g => new ContactWithMessage
            {
                contactId = g.ContactId,
                contactName = g.ContactName,
                imgUrlForContact = g.ImgUrl,
                isGroup = g.IsGroup,
                senderNameForMessageInGroup = g.LastMessage != null
                    ? _context.Users
                        .Where(u => u.Id == g.LastMessage.SenderId)
                        .Select(u => u.UserName)
                        .FirstOrDefault()
                    : null,
                messageText = g.LastMessage != null ? g.LastMessage.MessageText : string.Empty,
                timeOfMessage = g.LastMessage != null ? g.LastMessage.TimeOfSend : DateTime.MinValue,
                statusOfMessage = g.LastMessage != null ? g.LastMessage.MessageStatus : string.Empty,
                isMessageDeleted = g.LastMessage != null && g.LastMessage.IsDeleted.HasValue && g.LastMessage.IsDeleted.Value
            }).ToList();

            // Fetch individual contacts with the last message data
            var contactsWithLastMessages = await _context.Messages
                .AsNoTracking()
                .Where(x => (x.SenderId == userId || x.RecieverId == userId) && x.GroupId == null)
                .GroupBy(x => x.SenderId == userId ? x.RecieverId : x.SenderId)
                .Select(g => new
                {
                    ContactId = g.Key,
                    LastMessage = g.OrderByDescending(m => m.TimeOfSend).FirstOrDefault()
                })
                .Join(_context.Users.AsNoTracking(),
                    msg => msg.ContactId,
                    user => user.Id,
                    (msg, user) => new ContactWithMessage
                    {
                        contactId = user.Id,
                        contactName = user.UserName,
                        imgUrlForContact = user.imgURL,
                        isGroup = false,
                        senderName = msg.LastMessage.SenderId == userId ? "You" : user.UserName,
                        messageText = msg.LastMessage != null ? msg.LastMessage.MessageText : string.Empty,
                        timeOfMessage = msg.LastMessage != null ? msg.LastMessage.TimeOfSend : DateTime.MinValue,
                        statusOfMessage = msg.LastMessage != null ? msg.LastMessage.MessageStatus : string.Empty,
                        isMessageDeleted = msg.LastMessage != null && msg.LastMessage.IsDeleted.HasValue && msg.LastMessage.IsDeleted.Value
                    })
                .ToListAsync();

            // Merge both lists together
            var allContacts = groupContacts.Concat(contactsWithLastMessages)
                .OrderByDescending(c => c.timeOfMessage)
                .ToList();

            return allContacts;


            //var groupsWithLastMessagesThatUserIn = await _context.GroupMembers
            //    .AsNoTracking() // Avoids tracking since it's a read-only operation
            //    .Where(gm => gm.UserId == userId)
            //    .Include(gm => gm.Group) // Load related group data
            //    .Select(gm => new
            //    {
            //        GroupId = gm.Group.Id,
            //        GroupName = gm.Group.GroupName,
            //        ImgUrl = gm.Group.imgUrl,
            //        LastMessageData = _context.Messages
            //            .Where(m => m.GroupId == gm.Group.Id)
            //            .OrderByDescending(m => m.TimeOfSend)
            //            .Join(_context.Users, // Joining Users to fetch SenderName directly
            //                m => m.SenderId,
            //                u => u.Id,
            //                (m, u) => new
            //                {
            //                    TimeOfSend = m.TimeOfSend,
            //                    MessageText = m.MessageText,
            //                    IsDeleted = m.IsDeleted,
            //                    MessageStatus = m.MessageStatus,
            //                    SenderName = u.UserName ?? "Unknown" // Handle null sender names gracefully
            //                })
            //            .FirstOrDefault() // Fetch only the most recent message
            //    })
            //    .OrderByDescending(g => g.LastMessageData != null ? g.LastMessageData.TimeOfSend : DateTime.MinValue) // Order by last message time, handling potential nulls
            //    .ToListAsync();




            //var user = new ContactWithMessage();

            //var contactsWithLastMessage = await _context.Messages
            //    .AsNoTracking() // Avoids tracking changes since this is a read-only query
            //    .Where(x => (x.SenderId == userId || x.RecieverId == userId) && x.GroupId == null) // Filter messages between the user and other contacts
            //    .GroupBy(x => x.SenderId == userId ? x.RecieverId : x.SenderId) // Group by the other user’s ID
            //    .Select(g => new
            //    {
            //        ContactId = g.Key,
            //        LastMessage = g.OrderByDescending(m => m.TimeOfSend).FirstOrDefault() // Fetch the last message directly
            //    })
            //    .Join(_context.Users.AsNoTracking(), // Join with the Users table to fetch user details
            //        msg => msg.ContactId,
            //        user => user.Id,
            //        (msg, user) => new
            //        {
            //            ContactId = user.Id,
            //            ContactName = user.UserName,
            //            ImgUrl = user.imgURL,
            //            LastMessageData = new
            //            {
            //                TimeOfSend = msg.LastMessage.TimeOfSend,
            //                MessageText = msg.LastMessage.MessageText,
            //                IsDeleted = msg.LastMessage.IsDeleted,
            //                MessageStatus = msg.LastMessage.MessageStatus,
            //            }
            //        })
            //    .OrderByDescending(g => g.LastMessageData.TimeOfSend) // Order the contacts by the last message time
            //    .ToListAsync();



            //throw new NotImplementedException();
        }

        public Task<userDataInDetails> getUserDataAsync(string id)
        {
            throw new NotImplementedException();
        }
        // when user makes search on contact by phone or name 
        public Task<List<ContactWithMessage>> getUsersByNameOrPhoneAsync(string searchText)
        {
            throw new NotImplementedException();
        }

        public Task<bool> deleteChat(string deleteContactId)
        {
            throw new NotImplementedException();
        }

        public Task<userDataModel> updateUserDataAsync(userUpdateDataModel model)
        {
            throw new NotImplementedException();
        }
    }
}
