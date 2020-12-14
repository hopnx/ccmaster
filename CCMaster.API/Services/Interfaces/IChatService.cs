using CCMaster.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CCMaster.API.Services
{
    public interface IChatService
    {
        public Task SendMessages(ChatMessage request);
    }
}
