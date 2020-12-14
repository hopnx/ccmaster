using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CCMaster.API.Models;
using Microsoft.AspNetCore.Routing;

namespace CCMaster.API.Hubs.Chat
{
    public interface IChatClient
    {
        Task ReceiveMessage(ChatMessage message);
    }
    public class ChatHub:Hub<IChatClient>
    {
        static public string Route = "/hubs/chat";
        public async Task SendMessage(ChatMessage message)
        {
            await Clients.All.ReceiveMessage(message);            
        }
    }
}
