using CCMaster.API.Hubs.Chat;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CCMaster.API.Services
{
    public class ChatService: IChatService
    {
        private readonly IHubContext<ChatHub, IChatClient> _chatHub;
        public ChatService(IHubContext<ChatHub, IChatClient> chatHub)
        {
            _chatHub = chatHub;
        }
        public async Task SendMessages(Models.ChatMessage request)
        {
            await _chatHub.Clients.All.ReceiveMessage(request);
        }
    }
}
