using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CCMaster.API.Hubs.Chat;
using CCMaster.API.Models;
using CCMaster.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CCMaster.API.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _service;
        
        public ChatController(IChatService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task Messages(ChatMessage message)
        {
            // run some logic...

            await _service.SendMessages(message);
        }
    }
}
