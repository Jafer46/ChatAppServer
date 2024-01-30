using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatAppServer.Contracts;
using ChatAppServer.Interfaces;
using ChatAppServer.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using SignalRSwaggerGen.Attributes;

namespace ChatAppServer.Hubs
{
    [SignalRHub]
    public partial class ChatHub : Hub
    {
        private readonly IConfiguration _config;
        private readonly IEntity<User> _userServices;
        private readonly IEntity<Group> _groupServices;
        private readonly IEntity<Message> _messageServices;
        private readonly IEntity<GroupUser> _groupUserServices;
        public readonly IEntity<UserFriend> _userFreindServices;
        public ChatHub(IConfiguration config,
                  IEntity<User> userServices,
                  IEntity<Group> groupServices,
                  IEntity<Message> messageServices,
                  IEntity<GroupUser> groupUserServices,
                  IEntity<UserFriend> userFreindServices)
        {
            _config = config;
            _userServices = userServices;
            _groupServices = groupServices;
            _messageServices = messageServices;
            _groupUserServices = groupUserServices;
            _userFreindServices = userFreindServices;
        }
        public override async Task OnConnectedAsync()
        {
            HttpContext? hc = Context.GetHttpContext();

            if (hc != null)
            {
                string? Token = hc.Request.Query["access_token"];

                //set user IsOnline true when he connects or reconnects
                if (!string.IsNullOrWhiteSpace(Token))
                {
                    User? connectedUser = await _userServices.ReadFirst(x => x.Token == Token);
                    if (connectedUser != null)
                    {
                        connectedUser.ConnectionId = Context.ConnectionId;
                        connectedUser.IsOnline = true;
                        await _userServices.Update(connectedUser);
                    }
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            HttpContext? hc = Context.GetHttpContext();

            if (hc != null)
            {
                string? Token = hc.Request.Query["access_token"];

                //set user IsOnline false when he disconnects
                if (!string.IsNullOrWhiteSpace(Token))
                {
                    User? connectedUser = await _userServices.ReadFirst(x => x.Token == Token);
                    if (connectedUser != null)
                    {
                        connectedUser.ConnectionId = null;
                        connectedUser.IsOnline = false;
                        await _userServices.Update(connectedUser);
                    }
                }

            }

            await base.OnDisconnectedAsync(exception);
        }
    }

}