using chattingApp.DataAndContext;
using chattingApp.vModels;
using Microsoft.EntityFrameworkCore;
using Twilio.Rest.Serverless.V1.Service.Asset;

namespace chattingApp.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private readonly ITransferPhotosToPathWithStoreService _photoService;

        public UserService(ApplicationDbContext context, 
                            IAuthService authService, 
                            ITransferPhotosToPathWithStoreService photoService)
        {
            _context = context;
            _authService = authService;
            _photoService = photoService;
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
                        .Where(m => m.GroupId == gm.Group.Id && m.TimeOfSend > gm.UserJoinedAt && (m.TimeOfSend < gm.UserRemovedAt || gm.UserRemovedAt == null))
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
                messageText = g.LastMessage != null && g.LastMessage.IsDeleted.HasValue && !g.LastMessage.IsDeleted.Value ? g.LastMessage.MessageText : string.Empty,
                timeOfMessage = g.LastMessage != null ? g.LastMessage.TimeOfSend : DateTime.MinValue,
                statusOfMessage = g.LastMessage != null ? g.LastMessage.MessageStatus : string.Empty,
                isMessageDeleted = g.LastMessage != null && g.LastMessage.IsDeleted.HasValue && g.LastMessage.IsDeleted.Value
            }).ToList();

            // Fetch individual contacts with the last message data // there is error here 
            var contactsWithLastMessages = _context.Messages
                .AsNoTracking()
                .Where(x => (x.SenderId == userId || x.RecieverId == userId) && x.GroupId == null)
                .GroupBy(x => x.SenderId == userId ? x.RecieverId : x.SenderId)
                .Select(g => new
                {
                    ContactId = g.Key,
                    LastMessage = g.OrderByDescending(m => m.TimeOfSend).FirstOrDefault()
                })
                .AsEnumerable()
                .Join(_context.Users.AsNoTracking(),
                    msg => msg.ContactId,
                    user => user.Id,
                    (msg, user) => new ContactWithMessage
                    {
                        contactId = user.Id,
                        contactName = user.UserName,
                        contactPhoneNumber = user.PhoneNumber,
                        imgUrlForContact = user.imgURL,
                        isGroup = false,
                        senderName = msg.LastMessage.SenderId == userId ? "You" : user.UserName,
                        messageText = msg.LastMessage != null ? msg.LastMessage.MessageText : string.Empty,
                        timeOfMessage = msg.LastMessage != null ? msg.LastMessage.TimeOfSend : DateTime.MinValue,
                        statusOfMessage = msg.LastMessage != null ? msg.LastMessage.MessageStatus : string.Empty,
                        isMessageDeleted = msg.LastMessage != null && msg.LastMessage.IsDeleted.HasValue && msg.LastMessage.IsDeleted.Value
                    })
                .ToList();

            // Merge both lists together
            var allContacts = groupContacts.Concat(contactsWithLastMessages)
                .OrderByDescending(c => c.timeOfMessage)
                .ToList();

            return allContacts;
        }

        public async Task<userDataInDetails> getUserDataAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new userDataInDetails { message = "error, user id required!" };

            var result = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new userDataInDetails
                {
                    id = u.Id,
                    imgUrl = u.imgURL,
                    lastOnlineTime = u.lastOnlineTime,
                    name = u.UserName,
                    phoneNumber = u.PhoneNumber,
                    message = ""
                })
                .FirstOrDefaultAsync();

            if (result == null)
                return new userDataInDetails { message = "user not found" };

            return result;
                
        }
        // when user makes search on contact by phone or name 
        public async Task<List<ContactWithMessage>> getUsersByNameOrPhoneAsync(string searchText)
        {
            // Fetch all contacts for the user
            var result = await getContactsForUserAsync();

            // Return all contacts if the search text is empty
            if (string.IsNullOrEmpty(searchText))
                return result;

            // Check if the search text is a phone number (11 digits or less, all numeric)
            if (searchText.Length <= 11 && searchText.All(char.IsDigit))
            {
                // Filter by phone number
                result = result.Where(x => !string.IsNullOrEmpty(x.contactPhoneNumber) && x.contactPhoneNumber.Contains(searchText)).ToList();
            }
            else
            {
                // Filter by contact name
                result = result.Where(x => !string.IsNullOrEmpty(x.contactName) && x.contactName.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return result;
        }


        public async Task<deleteChatResult> deleteChatAsync(string deleteContactId)
        {
            var currentUserId = _authService.getUserId();

            var contact = await _context.Users.FirstOrDefaultAsync(x => x.Id == deleteContactId);
            if (contact == null) 
                return new deleteChatResult { message = "there is no contacts with this id",isDeletedSuccessfully = false };
            
            var messagesShouldDelete = await _context.Messages
                .Where(
                        m => m.GroupId == null &&
                        ((m.SenderId == currentUserId && m.RecieverId == deleteContactId) || 
                        (m.RecieverId == currentUserId && m.SenderId == deleteContactId))
                )
                .ToListAsync();

            if (messagesShouldDelete.Any() )
            {
                _context.Messages.RemoveRange(messagesShouldDelete);
                await _context.SaveChangesAsync();
                new deleteChatResult { isDeletedSuccessfully = true, message = "" };
            }

            return new deleteChatResult { isDeletedSuccessfully = true, message = ""};
        }

        public async Task<string> updateUserDataAsync(userUpdateDataModel model)
        {
            var currentUserId = _authService.getUserId();

            if(await _context.Users.FirstOrDefaultAsync(x => x.UserName == model.name) != null)
                return "this new user name used for another user!";

            // store the new img 
            var newImgPath = _photoService.GetPhotoPath(model.img);
            if (string.IsNullOrEmpty(newImgPath) || newImgPath.StartsWith("error, "))
                return "issue while storing image: " + newImgPath;


            // get this user 
            var modifiedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
            var oldImgPath = modifiedUser.imgURL;
            // update the user data
            modifiedUser.imgURL = newImgPath;
            modifiedUser.UserName = model.name;
            modifiedUser.NormalizedUserName = model.name.ToUpper();
            var changes = await _context.SaveChangesAsync();
            if (changes > 0)
                _photoService.DeleteFile(oldImgPath);

            return "";
        }
    }
}
