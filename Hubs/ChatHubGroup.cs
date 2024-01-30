using ChatAppServer.Contracts;
using ChatAppServer.Interfaces;
using ChatAppServer.Models;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver.Linq;

namespace ChatAppServer.Hubs
{
    public partial class ChatHub : IChatGroup
    {
        public async Task<bool> AddGroupUser(string groupId, string userId)
        {
            try
            {
                HttpContext? hc = Context.GetHttpContext();
                if (hc == null || userId == null)
                {
                    System.Console.WriteLine("hc or id are null");
                    return false;
                }

                string? Token = hc.Request.Query["access_token"];

                User? cuser = await _userServices.ReadFirst(x => x.Token == Token);
                User? user = await _userServices.ReadFirst(x => x.Id.ToString() == userId);

                if (cuser == null || user == null)
                {
                    System.Console.WriteLine("cuser or user are null");
                    return false;
                }
                if (cuser.Id != user.Id)
                {
                    if (await GroupContainsUser(groupId, userId) || !await GroupContainsUser(groupId, cuser.Id.ToString()))
                    {
                        System.Console.WriteLine("user or cuser exists");
                        return false;
                    }
                }

                GroupUser groupUser = new()
                {
                    GroupId = groupId,
                    UserId = userId
                };
                if (cuser.Id.ToString() == userId)
                {
                    System.Console.WriteLine("user is admin");
                    groupUser.IsAdmin = true;
                }
                GroupUser? newGroup = await _groupUserServices.Create(groupUser);
                if (newGroup != null)
                {
                    if (cuser.Id.ToString() != userId)
                    {
                        await SendMessage(cuser.Id.ToString(), null, null, $"{cuser.Username} added {user.Username}");
                    }
                    return true;
                }
                System.Console.WriteLine("couldnt create group user");
            }
            catch { }
            return false;
        }

        public async Task<GroupContract?> CreateGroup(string groupName)
        {
            try
            {
                Group? g = await _groupServices.ReadFirst(x => x.Title == groupName);
                if (g != null)
                {
                    return null;
                }
                HttpContext? hc = Context.GetHttpContext();
                if (hc == null)
                {
                    System.Console.WriteLine("hc was null");
                    return null;
                }

                string? Token = hc.Request.Query["access_token"];

                User? cuser = await _userServices.ReadFirst(x => x.Token == Token);

                if (cuser == null)
                {
                    System.Console.WriteLine("current user not found");
                    return null;
                }

                Group group = new Group()
                {
                    Title = groupName,
                    CreatedAt = DateTime.Now
                };
                Group? newGroup = await _groupServices.Create(group);
                if (newGroup != null)
                {
                    Group? groupCreated = await _groupServices.ReadFirst(x => x.Title == groupName);
                    if (groupCreated == null)
                    {
                        return null;
                    }
                    await AddGroupUser(groupCreated.Id.ToString(), cuser.Id.ToString());

                    GroupContract groupContract = new()
                    {
                        Id = groupCreated.Id.ToString(),
                        AvatarURL = groupCreated.AvatarUrl,
                        Title = groupCreated.Title
                    };
                    return groupContract;
                }
                System.Console.WriteLine("group could not be created");
            }
            catch { }
            return null;
        }

        public async Task<bool> DeleteGroup(string groupId)
        {
            HttpContext? hc = Context.GetHttpContext();
            if (hc == null)
            {
                return false;
            }

            string? Token = hc.Request.Query["access_token"];

            User? cuser = await _userServices.ReadFirst(x => x.Token == Token);

            if (cuser == null)
            {
                return false;
            }

            if (!await IsGroupAdmin(groupId, cuser.Id.ToString()))
            {
                return false;
            }

            if (!await _groupUserServices.Delete(x => x.GroupId == groupId))
            {
                return false;
            }

            if (!await _messageServices.Delete(x => x.GroupId == groupId))
            {
                return false;
            }

            return await _groupServices.Delete(x => x.Id.ToString() == groupId);
        }

        public async Task<GroupContract?> GetGroupById(string groupId)
        {
            System.Console.WriteLine("groupid frm getgbi: " + groupId);
            Group? group = await _groupServices.ReadFirst(x => x.Id.ToString() == groupId);
            if (group == null)
            {
                System.Console.WriteLine("group not found");
                return null;
            }
            GroupContract groupContract = new()
            {
                Id = group.Id.ToString(),
                AvatarURL = group.AvatarUrl,
                Title = group.Title,
            };
            System.Console.WriteLine(groupContract.Title);
            return groupContract;
        }

        public async Task<List<UserContract>?> GetGroupUsers(string groupId)
        {
            List<GroupUser> groupUsers = await _groupUserServices.ReadAll(x => x.GroupId == groupId);
            if (groupUsers == null)
            {
                return null;
            }
            List<User> users = new();
            foreach (var groupUser in groupUsers)
            {
                User? user = await _userServices.ReadFirst(u => u.Id.ToString() == groupUser.UserId);
                if (user == null) continue;
                users.Add(user);
            }
            if (users == null)
            {
                return null;
            }
            List<UserContract> userContracts = new();
            foreach (var user in users)
            {
                userContracts.Add(new UserContract
                {
                    Id = user.Id.ToString(),
                    AvatarUrl = user.AvatarUrl,
                    UserName = user.Username,
                    IsOnline = user.IsOnline
                });
            }
            return userContracts;
        }

        public async Task<List<GroupContract>?> GetUsergroups()
        {
            try
            {
                HttpContext? hc = Context.GetHttpContext();
                if (hc == null)
                {
                    return null;
                }

                string? Token = hc.Request.Query["access_token"];

                User? cuser = await _userServices.ReadFirst(x => x.Token == Token);

                if (cuser == null)
                {
                    return null;
                }
                List<GroupUser>? groupUsers = await _groupUserServices.ReadAll(x => x.UserId == cuser.Id.ToString());

                if (groupUsers == null)
                {
                    return null;
                }
                List<Group> groups = new();
                foreach (var groupUser in groupUsers)
                {
                    Group? group = await _groupServices.ReadFirst(x => x.Id.ToString() == groupUser.GroupId);
                    if (group == null)
                    {
                        continue;
                    }

                    groups.Add(group);
                }
                List<GroupContract> groupContracts = new();
                foreach (var g in groups)
                {
                    groupContracts.Add(new GroupContract()
                    {
                        Id = g.Id.ToString(),
                        AvatarURL = g.AvatarUrl,
                        Title = g.Title
                    });
                }
                return groupContracts;
            }
            catch { }
            return null;
        }

        public async Task<bool> GroupContainsUser(string groupId, string userId)
        {
            GroupUser? groupUser = await _groupUserServices
                         .ReadFirst(x => x.GroupId == groupId && x.UserId == userId);
            if (groupUser == null)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> IsGroupAdmin(string groupId, string userId)
        {
            GroupUser? groupUser = await _groupUserServices.ReadFirst(x => x.UserId == userId && x.GroupId == groupId);
            if (groupUser == null)
            {
                return false;
            }
            return groupUser.IsAdmin;
        }

        public async Task<bool> LeaveGroup(string groupId)
        {
            HttpContext? hc = Context.GetHttpContext();
            if (hc == null)
            {
                return false;
            }

            string? Token = hc.Request.Query["access_token"];

            User? cuser = await _userServices.ReadFirst(x => x.Token == Token);

            if (cuser == null)
            {
                return false;
            }

            return await _groupUserServices.Delete(x => x.UserId == cuser.Id.ToString() && x.GroupId == groupId);
        }
    }
}