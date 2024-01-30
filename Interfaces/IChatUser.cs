using ChatAppServer.Contracts;

namespace ChatAppServer.Interfaces
{
    public interface IChatUser
    {
        Task<List<UserContract>?> GetFriends();
        Task<UserContract?> GetUserById(string userId);
        Task<bool> IsUserOnline(string userId);
        Task<bool> AddFriend(string toBeFriendId);
        Task<bool> RemoveFriend(string freindId);
        Task<bool> IsFreind(string freindId);
        Task<List<UserContract>?> SearchUser(string query, int maxResult = 20);
    }
}