using chattingApp.vModels;

namespace chattingApp.Services
{
    public interface IUserService
    {
        // update last online time
        Task<userDataModel> updateUserDataAsync(userUpdateDataModel model);
        Task<userDataInDetails> getUserDataAsync(string id);
        Task<List<ContactWithMessage>> getUsersByNameOrPhoneAsync(string searchText);
        Task<List<ContactWithMessage>> getContactsForUserAsync();
        Task<bool> deleteChat(string deleteContactId);
    }
}

//- search by phone or name
//- show user info
//- get contacts for user ordered by last massages(you will know the contacts for the user by messages between sender and reciever)
//-remove contact for the user(delete chat)

