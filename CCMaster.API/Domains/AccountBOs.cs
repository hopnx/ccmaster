using CoreLibrary.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CCMaster.API.Domains
{
    public class RequestLogin:BaseRequest
    {
        public string User { get; set; }
        public string Password { get; set; }
    }
    public class ResponseLogin 
    {

    }
    public class RequestLogout:BaseRequest
    {
        public Guid UserId { get; set; }       
    }
    public class ResponseLogout
    {

    }
    public class RequestPlayerInfo : BaseRequest
    {
        public Guid Id { get; set; }
    }

    public class DOPlayer
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Rank { get; set; }
        public int Score { get; set; }
        public int TotalGame { get; set; }
        public int TotalWin { get; set; }
        public int TotalLose { get; set; }
        public int TotalDraw { get; set; }
    }
    public class DOAccount
    {
        public Guid Id {get;set;}
        public string UserName { get; set; }
        public virtual DOPlayer Player { get; set; }
    }
}
