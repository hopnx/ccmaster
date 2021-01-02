using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CCMaster.API.Domains;
using CCMaster.API.Hubs.Login;
using CCMaster.API.Models;
using CCMaster.API.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Driver;
using CoreLibrary.Base;
using CoreLibrary.Helper;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace CCMaster.API.Services
{
    public static partial class EntityExtension
    {
        static public DOLoginResult MapTo(this Account data)
        {
            return new DOLoginResult
            {
                Id = data.Id,
                PlayerId = data.PlayerId
            };
        }
    }
    public class LoginService : BaseService, ILoginService
    {
        private readonly string DM_ACCOUNT = "Account";
        private readonly string DM_ONLINE_ACCOUNT = "Online";
        private readonly IHubContext<LoginHub> _loginHub;
        private readonly IPlayerService _playerService;
        public LoginService(IDistributedCache cache
            , IMongoConfiguration mongoConfig
            , IHubContext<LoginHub> loginHub
            , IPlayerService playerService) : base(cache,mongoConfig)
        {
            _loginHub = loginHub;
            _playerService = playerService;
        }
        public async Task<BaseResponse<DOLoginResult>> Login(RequestLogin request)
        {
            BaseResponse<DOLoginResult> response = CreateResponse<DOLoginResult>();
            try
            {
                //check user
                bool userIsExist = await IsExistAsync(DM_ACCOUNT, request.User);
                if (!userIsExist)
                {
                    //check at mongoDB
                    IMongoCollection<Account> collectionAccount = MongoGetCollection<Account>();
                    Account account = collectionAccount.Find(document => document.UserName == request.User).FirstOrDefault();
                    if (account == null)
                    {
                        account = CreateNewAccount(request);
                    }
                    IMongoCollection<Player> collectionPlayer = MongoGetCollection<Player>();
                    Player player = collectionPlayer.Find(doc => doc.Name == account.UserName).FirstOrDefault();
                    if (player == null)
                    {
                        player = _playerService.CreatePlayer(account);
                    }
                    account.PlayerId = player.Id;
                    SaveAccount(account);
                }

                Account checkAccount = GetObject<Account>(DM_ACCOUNT, request.User);
                string md5Password = Hash.MD5Encode(request.Password);
                if (!checkAccount.Password.Equals(md5Password))
                {
                    response.OK = false;
                    response.Message = "Mật khẩu chưa trùng khớp";
                }
                else
                {
                    response.OK = true;
                    response.Message = "Đăng nhập thành công";
                    response.Data = checkAccount.MapTo();
                    checkAccount.LastLogin = DateTime.Now;
                    checkAccount.LastLogout = null;
                    SaveAccount(checkAccount);
                    CheckOnline(request);
                }
            }
            catch (Exception e)
            {
                ExceptionHandle(response, e);
            }
            return response;
        }
        private Account CreateNewAccount(RequestLogin request)
        {
            Account account = new Account
            {
                Id = Guid.NewGuid(),
                UserName = request.User,
                Password = Hash.MD5Encode(request.Password)
            };
            return account;
        }
        private void SaveAccount(Account account)
        {
            IMongoCollection<Account> collection = MongoGetCollection<Account>();
            Account old = collection.Find(acc => acc.Id == account.Id).FirstOrDefault();
            if (old == null)
                collection.InsertOneAsync(account);
            else
                collection.ReplaceOneAsync(acc => acc.Id == account.Id, account);

            SetObject(DM_ACCOUNT, account.UserName,account);
        }
        public void CheckOnline(RequestLogin request)
        {
            if (IsExist(DM_ONLINE_ACCOUNT, request.User))
            {
                string onlineConnectionId = Get(DM_ONLINE_ACCOUNT, request.User);
                if (!request.ConnectionId.Equals(onlineConnectionId))
                {
                    SendToClient(onlineConnectionId, "ReceiveDisconnectCommand");
                }
            }
            int expiredSecond = 300; //5 minutes
            SetWithExpire(DM_ONLINE_ACCOUNT, request.User, request.ConnectionId, expiredSecond);
        }
        private void SendToClient(string connectionId, string message)
        {
            _loginHub.Clients.Clients(connectionId).SendAsync(message);
        }
        public BaseResponse<Account> Logout(RequestLogout request)
        {
            throw new NotImplementedException();
        }
    }
}
