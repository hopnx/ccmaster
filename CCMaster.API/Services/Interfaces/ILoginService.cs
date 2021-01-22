using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CCMaster.API.Domains;
using CCMaster.API.Models;
using CoreLibrary.Base;

namespace CCMaster.API.Services.Interfaces
{
    public interface ILoginService
    {
        public Task<BaseResponse<DOLoginResult>> Login(RequestLogin request);
        public Task<BaseResponse<DOLoginResult>> SDKLogin(RequestSDKLogin request);
        public BaseResponse<Account> Logout(RequestLogout request);
    }
}
