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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CCMaster.API.Services
{
    public class RankDefinition
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public int No { get; set; }
        public string Name { get; set; }
        public int From { get; set; }
        public int To { get; set; }
    }
    public class DORankDefinition
    {
        public int No { get; set; }
        public string Name { get; set; }
    }
    public static partial class EntityExtension
    {
        static public DORankDefinition MapTo (this RankDefinition data)
        {
            return new DORankDefinition
            {
                No = data.No,
                Name = data.Name,
            };
        }
    }
    public class GameConfigService: BaseService, IGameConfigService
    {
        public List<RankDefinition> Ranks { get; private set; }
        public GameConfigService(IDistributedCache cache, IMongoConfiguration mongoConfig) : base(cache, mongoConfig)
        {
            LoadRankDefinition();
        }
        private async void LoadRankDefinition()
        {
            IMongoCollection<RankDefinition> collection = MongoGetCollection<RankDefinition>();
            Ranks = await collection.Find(Builders<RankDefinition>.Filter.Empty).ToListAsync();
            if (Ranks.Count == 0)
                InitRankDefinition();
        }
        private async void InitRankDefinition()
        {
            Ranks.Add(new RankDefinition
            {
                No = 1,
                Name = "Tập sự",
                From = 0,
                To = 1700,
            }); 
            Ranks.Add(new RankDefinition
            {
                No = 2,
                Name = "Kỳ thủ",
                From = 1701,
                To = 1800,
            });
            Ranks.Add(new RankDefinition
            {
                No=3,
                Name="Đại sư",
                From = 1801,
                To = 3000,
            });
            IMongoCollection<RankDefinition> collection = MongoGetCollection<RankDefinition>();
            await collection.InsertManyAsync(Ranks);
        }
        public DORankDefinition GetRank(int score)
        {
            RankDefinition rank = Ranks.Where(r => r.From < score && r.To >= score).FirstOrDefault();
            if (rank == null)
                return new DORankDefinition
                {
                    No = 0,
                    Name = "Không xác định",
                };
            else
                return rank.MapTo();
        }
    }
}
