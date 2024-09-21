using chattingApp.vModels;

namespace chattingApp.Services
{
    public interface IGroupService
    {
        public Task<createGroupResult> createGroup(newGroupModel model);
        public Task<createGroupResult> updateGroup(updateGroupModel model);
        public Task<string> deleteGroup(string groupId);
        public Task<string> addMember(groupUserIds model);
        public Task<string> removeMember(groupUserIds model);


    }
}
