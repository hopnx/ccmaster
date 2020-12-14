using CCMaster.API.Domains;
using CCMaster.API.Hubs;
using CCMaster.API.Models;
using CoreLibrary.Base;
using CoreLibrary.Helper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CCMaster.API.Services
{
    static public partial class EntityExtension
    {
        static public PlayerInfo MapToInfo(this Player data)
        {
            return new PlayerInfo
            {
                Id = data.Id,
                Name = data.Name
            };
        }
    }
    public class PlayerService : BaseService, IPlayerService
    {
        private readonly string DM_PLAYER = "Player";
        private readonly IHubContext<PlayerHub> _playerHub;
        private Dictionary<Guid, Player> _dicPlayer;
        private readonly IGameConfigService _gameConfigService;
        private readonly IMongoCollection<Player> _collectionPlayer;

        public PlayerService(IDistributedCache cache, IHubContext<PlayerHub> playerHub, IMongoConfiguration mongoConfig,IGameConfigService gameConfigService) : base(cache, mongoConfig)
        {
            _playerHub = playerHub;
            _dicPlayer = new Dictionary<Guid, Player>();
            _gameConfigService = gameConfigService;
            _collectionPlayer = MongoGetCollection<Player>();
        }
        public async Task<Player> GetPlayer(Guid playerId)
        {
            if (_dicPlayer.ContainsKey(playerId))
                return _dicPlayer.GetValueOrDefault(playerId);

            Player player = await LoadPlayerFromCache(playerId);
            if (player == null)
                player = await LoadPlayerFromMongoDB(playerId); 
            return player;
        }
        private async Task<Player> LoadPlayerFromCache(Guid id)
        {
            Player player = null;
            Boolean check = await IsExistAsync(DM_PLAYER,id.ToString());
            if (check)
                player = GetObject<Player>(DM_PLAYER, id.ToString());
            if (player != null)
                _dicPlayer.Add(player.Id, player);
            return player;
        }
        private async Task<Player> LoadPlayerFromMongoDB(Guid id)
        {
            Player player = (await _collectionPlayer.FindAsync(data => data.Id == id)).FirstOrDefault();
            if (player != null)
            {
                SavePlayerToCache(player);
                _dicPlayer.Add(player.Id, player);
            }
            return player;
        }
        private void SavePlayerToCache(Player player)
        {
            SetObject(DM_PLAYER, player.Id.ToString(), player);
            SetObject(DM_PLAYER, player.Name, player);
            _dicPlayer.Remove(player.Id);
            _dicPlayer.Add(player.Id, player);
        }
        public async Task<BaseResponse<DOPlayer>> GetPlayerInfo (RequestPlayerInfo request)
        {
            BaseResponse<DOPlayer> response = CreateResponse<DOPlayer>();
            try
            {
                Player player = await GetPlayer(request.Id);
                if (player == null)
                {
                    response.Message = "Người chơi không tồn tại";
                    return response;
                }
                response.Data = player.MapTo();
                response.OK = true;
            }
            catch(Exception e)
            {
                ExceptionHandle(response, e);
            }
            return response;
        }
        public void SavePlayer(Player player)
        {
            if (player != null)
            {
                player.Rank = _gameConfigService.GetRank(player.Score).Name;
                SavePlayerToCache(player);
                SavePlayerToMongoDB(player);
            }
        }
        private void SavePlayerToMongoDB(Player player)
        {
            Player old = _collectionPlayer.Find(p => p.Id == player.Id).FirstOrDefault();
            if (old == null)
                _collectionPlayer.InsertOneAsync(player);
            else
                _collectionPlayer.ReplaceOneAsync(p => p.Id == player.Id, player);
        }
        public Player CreatePlayer(Account account)
        {
            Player player = new Player
            {
                Id = Guid.NewGuid(),
                Name = account.UserName,
                Score = 1600,
                Coin = 10000,
                Rank = _gameConfigService.GetRank(1600).Name,
                TotalDraw = 0,
                TotalGame = 0,
                TotalLose = 0,
                TotalWin = 0,
            };
            SavePlayer(player);
            return player;
        }      

    }
}
