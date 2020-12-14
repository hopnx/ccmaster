using CCMaster.API.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreLibrary.Base;
using CCMaster.API.Models;

namespace CCMaster.API.Services
{
    public interface IPlayerService
    {
        public Task<BaseResponse<DOPlayer>> GetPlayerInfo(RequestPlayerInfo request);
        public Task<Player> GetPlayer(Guid playerId);
        public void SavePlayer(Player player);
        public Player CreatePlayer(Account account);
    }
}
