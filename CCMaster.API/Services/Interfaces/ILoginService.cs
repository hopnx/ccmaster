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
        public Task<BaseResponse<DOAccount>> Login(RequestLogin request);
        public BaseResponse<Account> Logout(RequestLogout request);
    }
}
