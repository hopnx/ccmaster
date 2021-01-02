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
        public int RankIndex { get; set; }
        public string RankLabel { get; set; }
        public int StarIndex { get; set; }
        public int From { get; set; }
        public int To { get; set; }
    }
    public class DORankDefinition
    {
        public int No { get; set; }
        public int RankIndex { get; set; }
        public string RankLabel { get; set; }
        public int StarIndex { get; set; }
    }
    public static partial class EntityExtension
    {
        static public DORankDefinition MapTo (this RankDefinition data)
        {
            return new DORankDefinition
            {
                No = data.No,
                RankIndex = data.RankIndex,
                RankLabel = data.RankLabel,
                StarIndex = data.StarIndex,
            };
        }
    }
    public class GameConfigService: BaseService, IGameConfigService
    {
        public const int START_SCORE = 1200;
        public const int START_COIN = 1000;
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
            int no = 1;
            Ranks.Add(new RankDefinition
            {
                No = no++,
                From = 0,
                To = 1399,
                RankLabel = "Tập sự",
                RankIndex = 1,
                StarIndex = 1,
            });
            Ranks.Add(new RankDefinition
            {
                No = no++,
                From = 1400,
                To = 1499,
                RankLabel = "Tập sự",
                RankIndex = 1,
                StarIndex = 2,
            });
            Ranks.Add(new RankDefinition
            {
                No = no++,
                From = 1500,
                To = 1599,
                RankLabel = "Tập sự",
                RankIndex = 1,
                StarIndex = 3,
            });
            Ranks.Add(new RankDefinition
            {
                No = no++,
                From = 1600,
                To = 1699,
                RankLabel = "Kỳ thủ",
                RankIndex = 2,
                StarIndex = 1,
            });
            Ranks.Add(new RankDefinition
            {
                No = no++,
                From = 1700,
                To = 1799,
                RankLabel = "Kỳ thủ",
                RankIndex = 2,
                StarIndex = 2,
            });
            Ranks.Add(new RankDefinition
            {
                No = no++,
                From = 1800,
                To = 1899,
                RankLabel = "Kỳ thủ",
                RankIndex = 2,
                StarIndex = 3,
            });
            Ranks.Add(new RankDefinition
            {
                No = no++,
                From = 1900,
                To = 1999,
                RankLabel = "Đại sư",
                RankIndex = 3,
                StarIndex = 1,
            });
            Ranks.Add(new RankDefinition
            {
                No = no++,
                From = 2000,
                To = 2099,
                RankLabel = "Đại sư",
                RankIndex = 3,
                StarIndex = 2,
            });
            Ranks.Add(new RankDefinition
            {
                No = no++,
                From = 2100,
                To = 2199,
                RankLabel = "Đại sư",
                RankIndex = 3,
                StarIndex = 3,
            });
            Ranks.Add(new RankDefinition
            {
                No = no++,
                From = 2200,
                To = 100000,
                RankLabel = "Kỳ tiên",
                RankIndex = 3,
                StarIndex = 3,
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
                    RankLabel = "Không xác định",
                    RankIndex = 0,
                    StarIndex = 0,
                };
            else
                return rank.MapTo();
        }
    }
}
