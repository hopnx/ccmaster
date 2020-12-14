using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CCMaster.API.Models
{
    public class ClientConfiguration
    {
        public string Url { get; set; }
    }
    public class RedisConfiguration
    {
        public string Configuration { get; set; }
        public string InstanceName { get; set; }
    }
    public class MongoConfiguration : IMongoConfiguration
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface IMongoConfiguration
    {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}
