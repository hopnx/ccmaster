using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreLibrary.Base;
using CCMaster.API.Models;
using CCMaster.API.Services;
using Microsoft.AspNetCore.SignalR;
using CCMaster.API.Domains;

namespace CCMaster.API.Hubs
{
    public interface IPlayerClient
    {
        Task ReceivePlayerInfo(BaseResponse<DOPlayer> response);
    }

    public class PlayerHub:Hub<IPlayerClient>
    {
        static readonly public string Route = "hubs/player";
        readonly IPlayerService _service;
        public PlayerHub(IPlayerService service)
        {
            _service = service;
        }
        public async Task RequestPlayerInfo(RequestPlayerInfo request)
        {
            request.ConnectionId = Context.ConnectionId;
            BaseResponse<DOPlayer> response = await _service.GetPlayerInfo(request);
            _ = Clients.Clients(request.ConnectionId).ReceivePlayerInfo(response);
        }
    }
}
