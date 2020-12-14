using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CCMaster.API.Models
{
    public class ChatMessage
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Message { get; set; }
    }
}
