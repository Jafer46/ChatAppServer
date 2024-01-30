using ChatAppServer.Contracts;

namespace ChatAppServer.Interfaces
{
    public interface IChatGroup
    {
        Task<GroupContract?> CreateGroup(string groupName);
        Task<bool> DeleteGroup(string groupId);
        Task<bool> AddGroupUser(string channelId, string userId);
        Task<bool> LeaveGroup(string groupId);
        Task<bool> GroupContainsUser(string groupId, string userId);
        Task<List<UserContract>?> GetGroupUsers(string groupId);
        Task<List<GroupContract>?> GetUsergroups();
        Task<GroupContract?> GetGroupById(string groupId);
        Task<bool> IsGroupAdmin(string groupId, string userId);
    }
}