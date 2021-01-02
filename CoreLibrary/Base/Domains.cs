using System;
using System.Collections.Generic;
using System.Text;

namespace CoreLibrary.Base
{
    public interface IBaseResponse
    {
        public bool OK { get; set; }
        public string Message { get; set; }
        public string Code { get; set; }
    }
    public class BaseResult
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
    }
    public class BaseResponse<T> : IBaseResponse
    {
        public bool OK { get; set;}
        public string Message { get; set;}
        public string Code { get; set;}
        public T Data { get; set; }
        public List<T> List { get; set; }
    }
    public class BaseRequest
    {
        public string ConnectionId { get; set; }
        public string Command { get; set; }

    }
}
