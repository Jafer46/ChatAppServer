using ChatAppServer.Contracts;
using ChatAppServer.Interfaces;
using ChatAppServer.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualBasic;

namespace ChatAppServer.Hubs
{
    public partial class ChatHub : IChatUser
    {
        public async Task<bool> AddFriend(string toBeFreindId)
        {
            try
            {

                HttpContext? hc = Context.GetHttpContext();
                if (hc == null)
                {
                    return false;
                }
                var token = hc.Request.Query["access_token"];
                User? cuser = await _userServices.ReadFirst(x => x.Token == token);
                if (cuser == null)
                {
                    return false;
                }
                UserFriend userFriend = new();
                userFriend.CreatedAt = DateTime.Now;
                userFriend.UserId = cuser.Id.ToString();
                userFriend.UserFreindId = toBeFreindId;
                var fr = await _userFreindServices.Create(userFriend);
                if (fr == null)
                {
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }

        }

        public async Task<List<UserContract>?> GetFriends()
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
                List<UserFriend> userFriends = await _userFreindServices
                             .ReadAll(x => x.UserId == cuser.Id.ToString() || x.UserFreindId == cuser.Id.ToString());
                if (userFriends == null)
                {
                    return null;
                }
                List<UserContract> friends = new();
                foreach (var userFriend in userFriends)
                {
                    if (userFriend.UserId == cuser.Id.ToString())
                    {
                        var user = await GetUserById(userFriend.UserFreindId!);
                        if (user == null) continue;
                        friends.Add(user);
                    }
                    else
                    {
                        var user = await GetUserById(userFriend.UserId!);
                        if (user == null) continue;
                        friends.Add(user);
                    }
                }

                return friends;
            }
            catch { return null; }

        }
        public async Task<UserContract?> GetUserById(string userId)
        {
            User? user = await _userServices.ReadFirst(u => u.Id.ToString() == userId);
            if (user == null)
            {
                System.Console.WriteLine("user not found");
                return null;
            }
            UserContract userContract = new()
            {
                Id = user.Id.ToString(),
                AvatarUrl = user.AvatarUrl,
                UserName = user.Username,
                IsOnline = user.IsOnline
            };
            return userContract;
        }
        public async Task<bool> IsUserOnline(string userId)
        {
            User? user = await _userServices.ReadFirst(x => x.Id.ToString() == userId);
            if (user == null)
            {
                return false;
            }
            return user.IsOnline;
        }

        public async Task<bool> RemoveFriend(string freindId)
        {
            try
            {
                HttpContext? hc = Context.GetHttpContext();
                if (hc == null)
                {
                    return false;
                }
                var token = hc.Request.Query["access_token"];
                User? cuser = await _userServices.ReadFirst(u => u.Token == token);
                if (cuser == null)
                {
                    return false;
                }
                if (!await _userFreindServices
                   .Delete(x => (x.UserId == cuser.Id.ToString() && x.UserFreindId == freindId) ||
                   (x.UserId == freindId && x.UserFreindId == cuser.Id.ToString())))
                {
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<UserContract>?> SearchUser(string query, int maxResult = 20)
        {
            try
            {

                HttpContext? hc = Context.GetHttpContext();

                if (hc == null)
                {
                    System.Console.WriteLine("hc not found in search user");
                    return null;
                }
                var token = hc.Request.Query["access_token"];
                User? cuser = await _userServices.ReadFirst(u => u.Token == token.ToString());

                if (cuser == null)
                {
                    System.Console.WriteLine("current user not found search user");
                    return null;
                }

                IEnumerable<User>? users = (await _userServices.ReadAll(x =>
                x.Username.Contains(query, StringComparison.InvariantCultureIgnoreCase)
                || x.Email.Contains(query, StringComparison.InvariantCultureIgnoreCase)))
                .OrderBy(x => x.Username).Take(maxResult);

                if (users == null)
                {
                    System.Console.WriteLine("searched user not found in search user");
                    return null;
                }
                List<UserContract> userContracts = new();
                foreach (var user in users)
                {
                    if (user.Id == cuser.Id) continue;
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
            catch (Exception e) { System.Console.WriteLine(e.Message); }
            System.Console.WriteLine("something went wring in search user");
            return null;

        }

        public async Task<bool> IsFreind(string freindId)
        {
            try
            {
                HttpContext? hc = Context.GetHttpContext();
                if (hc == null)
                {
                    System.Console.WriteLine("Isfreind: connection string not found");
                    return false;
                }
                var token = hc.Request.Query["access_token"];
                User? cuser = await _userServices.ReadFirst(u => u.Token == token);
                if (cuser == null)
                {
                    System.Console.WriteLine("isFreind: Connected user not found");
                    return false;
                }
                UserFriend? userFreind =
                           await _userFreindServices.ReadFirst(x =>
                           (x.UserId == cuser.Id.ToString() && x.UserFreindId == freindId) ||
                           (x.UserId == freindId && x.UserFreindId == cuser.Id.ToString()));
                if (userFreind == null)
                {
                    System.Console.WriteLine("isFreind: user freind was not found");
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}