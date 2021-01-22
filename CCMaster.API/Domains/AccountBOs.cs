using CoreLibrary.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CCMaster.API.Domains
{
    public class RequestSDKLogin : BaseRequest
    {
        public string Id { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
    public class RequestLogin:BaseRequest
    {
        public string Id { get; set; }
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
        public string RankLabel { get; set; }
        public int RankIndex { get; set; }
        public int StarIndex { get; set; }
        public int Score { get; set; }
        public int Coin { get; set; }
        public int TotalGame { get; set; }
        public int TotalWin { get; set; }
        public int TotalLose { get; set; }
        public int TotalDraw { get; set; }
    }
    public class DOLoginResult
    {
        public Guid Id {get;set;}
        public string SDKId { get; set; }
        public Guid  PlayerId { get; set; }
    }
}
