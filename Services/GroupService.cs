using chattingApp.DataAndContext;
using chattingApp.vModels;

namespace chattingApp.Services
{
    public class GroupService: IGroupService
    {
        private readonly IAuthService _authService;
        private readonly ApplicationDbContext _context;
        public GroupService(IAuthService authService, ApplicationDbContext context)
        {
            _authService = authService;
            _context = context;
        }
        public Task<createGroupResult> createGroup(newGroupModel model)
        {
            // not need authentication for user to access group
            throw new NotImplementedException();
        }
        public Task<createGroupResult> updateGroup(updateGroupModel model)
        {
            throw new NotImplementedException();
        }
        public Task<string> deleteGroup(string groupId)
        {
            throw new NotImplementedException();
        }
        public Task<string> addMember(groupUserIds model)
        {
            throw new NotImplementedException();
        }
        public Task<string> removeMember(groupUserIds model)
        {
            throw new NotImplementedException();
        }


        // helping functions
        private bool IsUserAuthenticatedForGroup(int groupId)
        {
            var userId = _authService.getUserId();
            var group = _context.Groups.FirstOrDefault(g => g.Id == groupId);

            if(userId != null && group != null && group.CreatedById == userId)
                return true;
            return false;
        }
    }
}

