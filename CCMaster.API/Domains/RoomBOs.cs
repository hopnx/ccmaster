using CCMaster.API.Models;
using CoreLibrary.Base;
using System;

namespace CCMaster.API.Domains
{
    
    public class RequestBoardList : BaseRequest
    {
        public string Room { get; set; }
    }
    public class RequestPlayerList: BaseRequest
    {
        public string Room { get; set; }
    }
    public class RequestCreateBoard : BaseRequest
    {
        public Guid PlayerId { get; set; }
        public string Room { get; set; }
    }
    public class RequestJoinBoard : BaseRequest
    {
        public Guid PlayerId { get; set; }
        public string Room { get; set; }
        public Guid BoardId { get; set; }
    }
}
