using ChatAppServer.Contracts;

namespace ChatAppServer.Interfaces
{
    public interface IChatMessage
    {
        Task<bool> SendMessage(string senderId, string recieverId, string groupId, string text);
        Task<bool> SetMessageAsSeen(string messageId);
        Task<bool> UpdateMessage(MessageContract message);
        //Task<bool> DeleteMessage(string messageId);
        Task<List<MessageContract>?> GetFriendMessageHistry(string friendId, int page = 1, int pageSize = 20);
        Task<List<MessageContract>?> GetGroupMessageHistry(string groupId, int page = 1, int pageSize = 20);
    }
}