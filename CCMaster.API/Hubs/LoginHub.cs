﻿using System.Threading.Tasks;
using CCMaster.API.Domains;
using CCMaster.API.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using CoreLibrary.Base;
using CCMaster.API.Models;

namespace CCMaster.API.Hubs.Login
{
    public interface ILoginHub
    {
        Task ReceiveLogin(BaseResponse<DOLoginResult> response);
        Task Disconnect();
    }

    public class LoginHub : Hub<ILoginHub>
    {
        static readonly public string Route = "hubs/login";
        readonly ILoginService _service;
        public LoginHub(ILoginService service)
        {
            _service = service;
        }
        public async Task RequestLogin(RequestLogin request)
        {
            request.ConnectionId = Context.ConnectionId;
            BaseResponse<DOLoginResult> response  = await _service.Login(request);
            _= Clients.Clients(request.ConnectionId).ReceiveLogin(response);
        }       
        public void DisconnectClient(string connectionId)
        {
            _ = Clients.Clients(connectionId).Disconnect();
        }
    }
}
