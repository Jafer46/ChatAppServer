using ChatAppServer.Contracts;
using ChatAppServer.Interfaces;
using ChatAppServer.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace ChatAppServer.Hubs
{
    public partial class ChatHub : IChatMessage
    {
        public async Task<List<MessageContract>?> GetFriendMessageHistry(string friendId, int page = 1, int pageSize = 20)
        {
            try
            {
                HttpContext? hc = Context.GetHttpContext();
                if (hc == null)
                {
                    return null;
                }
                var token = hc.Request.Query["access_token"];
                User? cuser = await _userServices.ReadFirst(u => u.Token == token);
                if (cuser == null)
                {
                    return null;
                }

                IEnumerable<Message>? messages =
                            await _messageServices.ReadAll(m =>
                            (m.SenderId == cuser.Id.ToString() && m.RecieverId == friendId) ||
                            (m.SenderId == friendId && m.RecieverId == cuser.Id.ToString()));
                if (messages == null)
                {
                    return null;
                }
                List<Message> messageList = messages
                                       .OrderByDescending(x => x.DateSent)
                                       .Skip((page - 1) * pageSize)
                                       .Take(pageSize)
                                       .ToList();

                List<MessageContract> messageContracts = new();
                foreach (var message in messageList)
                {
                    messageContracts.Add(new MessageContract()
                    {
                        Id = message.Id.ToString(),
                        SenderId = message.SenderId,
                        RecieverId = message.RecieverId,
                        GroupId = message.GroupId,
                        Text = message.Text,
                        Sent = message.Sent,
                        Seen = message.Seen,
                        DateSent = message.DateSent
                    });
                }
                return messageContracts;
            }
            catch { }
            return null;

        }
        public async Task<List<MessageContract>?> GetGroupMessageHistry(string groupId, int page = 1, int pageSize = 20)
        {
            System.Console.WriteLine("group id: {0}", groupId);
            try
            {
                IEnumerable<Message>? messages = await _messageServices.ReadAll(m => m.GroupId == groupId);
                if (messages == null)
                {
                    return null;
                }
                List<Message>? messageList = messages
                                 .OrderByDescending(x => x.DateSent)
                                 .Skip((page - 1) * pageSize)
                                 .Take(pageSize).ToList();
                List<MessageContract> messageContracts = new();
                foreach (var message in messageList)
                {
                    messageContracts.Add(new MessageContract()
                    {
                        Id = message.Id.ToString(),
                        SenderId = message.SenderId,
                        RecieverId = message.RecieverId,
                        GroupId = message.GroupId,
                        Sent = message.Sent,
                        Seen = message.Seen,
                        Text = message.Text,
                        DateSent = message.DateSent
                    });
                }
                return messageContracts;
            }
            catch { }
            return null;
        }

        public async Task<bool> SendMessage(string? senderId, string? recieverId, string? groupId, string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                System.Console.WriteLine("text or senderid is empty");
                return false;
            }
            Message message = new()
            {
                RecieverId = recieverId,
                GroupId = groupId,
                SenderId = senderId,
                Text = text,
                DateSent = DateTime.UtcNow,
                Sent = true
            };
            System.Console.WriteLine("new message prepared");
            Message sentMessage = await _messageServices.Create(message);
            if (sentMessage != null)
            {
                // Message? sentMessage = new();
                // System.Console.WriteLine("trying to send the message");
                // if (string.IsNullOrEmpty(senderId))
                // {
                //     sentMessage = await _messageServices.ReadLast(x => x.GroupId == groupId);
                // }
                // else
                // {
                //     sentMessage = await _messageServices.ReadLast(x => x.SenderId == senderId);
                // }

                if (sentMessage == null)
                {
                    System.Console.WriteLine("Message couldn't fetch after saving");
                    return false;
                }
                System.Console.WriteLine("Message was not null");
                MessageContract messageContract = new()
                {
                    Id = sentMessage.Id.ToString(),
                    SenderId = sentMessage.SenderId,
                    RecieverId = sentMessage.RecieverId,
                    GroupId = sentMessage.GroupId,
                    Sent = sentMessage.Sent,
                    Seen = sentMessage.Seen,
                    Text = sentMessage.Text,
                    DateSent = sentMessage.DateSent
                };
                if (string.IsNullOrEmpty(messageContract.GroupId) && !string.IsNullOrEmpty(messageContract.RecieverId))
                {
                    try
                    {
                        List<User> users = new List<User>();

                        var user1 = await _userServices.ReadFirst(u => u.Id.ToString() == message.SenderId);
                        var user2 = await _userServices.ReadFirst(u => u.Id.ToString() == message.RecieverId);
                        if (user1 == null || user2 == null)
                        {
                            return false;
                        }
                        users.Add(user1);
                        users.Add(user2);
                        if (users == null)
                        {
                            return false;
                        }
                        foreach (var user in users)
                        {
                            if (string.IsNullOrEmpty(user.ConnectionId)) continue;
                            await Clients.Client(user.ConnectionId).SendAsync("RecieveMessage", messageContract);
                        }
                    }
                    catch { System.Console.WriteLine("error occurd while notifiying other freinds"); }
                    return true;
                }
                else if (!string.IsNullOrEmpty(messageContract.GroupId) && string.IsNullOrEmpty(messageContract.RecieverId))
                {
                    System.Console.WriteLine("message is being sent");
                    List<UserContract>? users = await GetGroupUsers(messageContract.GroupId);
                    if (users is null)
                    {
                        System.Console.WriteLine("group users weren't found");
                        return false;
                    }
                    foreach (var user in users)
                    {
                        try
                        {
                            var databaseUser = await _userServices.ReadFirst(x => x.Id.ToString() == user.Id);
                            if (string.IsNullOrEmpty(databaseUser!.ConnectionId)) continue;
                            await Clients.Client(databaseUser.ConnectionId).SendAsync("ReceiveMessage", messageContract);
                        }
                        catch { System.Console.WriteLine("error occurd while notifiying other users"); }
                    }

                    return true;
                }
            }
            System.Console.WriteLine("message was not created");
            return false;
        }

        public async Task<bool> SetMessageAsSeen(string messageid)
        {
            Message? message = await _messageServices.ReadFirst(m => m.Id.ToString() == messageid);
            if (message == null)
            {
                return false;
            }
            message.Seen = true;
            MessageContract messageContract = new()
            {
                Id = message.Id.ToString(),
                SenderId = message.SenderId,
                RecieverId = message.RecieverId,
                GroupId = message.GroupId,
                Sent = message.Sent,
                Seen = message.Seen,
                DateSent = message.DateSent,
                Text = message.Text
            };
            if (!await UpdateMessage(messageContract))
            {
                return false;
            }
            return true;

        }

        public async Task<bool> UpdateMessage(MessageContract? message)
        {
            if (message == null)
            {
                return false;
            }
            Message? dataBaseMassage = await _messageServices.ReadFirst(x => x.Id.ToString() == message.Id);
            if (dataBaseMassage == null)
            {
                return false;
            }
            dataBaseMassage.Seen = message.Seen;
            dataBaseMassage.Text = message.Text;
            if (!await _messageServices.Update(dataBaseMassage))
            {
                System.Console.WriteLine("message was couldn't update!");
                return false;
            }
            try
            {
                List<User>? users = new();
                if (message.RecieverId != null && message.GroupId == null)
                {
                    var user1 = await _userServices.ReadFirst(u => u.Id.ToString() == message.SenderId);
                    var user2 = await _userServices.ReadFirst(u => u.Id.ToString() == message.RecieverId);
                    if (user1 == null || user2 == null)
                    {
                        return false;
                    }
                    users.Add(user1);
                    users.Add(user2);
                    if (users == null)
                    {
                        return false;
                    }
                    foreach (var user in users)
                    {
                        await Clients.Client(user.ConnectionId!).SendAsync("UpdateMessage", message);
                    }
                    return true;
                }
                else if (message.RecieverId == null && message.GroupId != null)
                {
                    var userContracts = await GetGroupUsers(message.GroupId);
                    if (userContracts == null)
                    {
                        return false;
                    }
                    foreach (var user in userContracts)
                    {
                        var databaseUser = await _userServices.ReadFirst(x => x.Id.ToString() == user.Id);
                        if (databaseUser!.ConnectionId == null) continue;
                        await Clients.Client(databaseUser.ConnectionId!).SendAsync("UpdateMessage", message);
                    }
                    return true;
                }

            }
            catch { }

            return false;
        }

        // public async Task<bool> DeleteMessage(string messageId)
        // {
        //     throw new NotImplementedMethod();
        // }
    }

}