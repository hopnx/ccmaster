using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Markup;
using CCMaster.API.Domains;
using Microsoft.AspNetCore.SignalR;
using CCMaster.API.Models;
using MongoDB.Driver;
using CoreLibrary.Base;
using CoreLibrary.Helper;

namespace CCMaster.API.Services
{
    public class ExceptionMongoDBNotInitialization : Exception
    {

    }
    public class BaseService
    {
        protected IDistributedCache cacheDB;
        protected IMongoClient mongoClient;
        protected IMongoDatabase mongoDB;

        //var database = client.GetDatabase(settings.DatabaseName);
        protected IMongoConfiguration _mongoConfig;
        public BaseService(IDistributedCache cache, IMongoConfiguration mongoConfig = null)
        {
            cacheDB = cache;
            if (mongoConfig != null)
            {
                _mongoConfig = mongoConfig;
                mongoClient = new MongoClient(mongoConfig.ConnectionString);
                mongoDB = mongoClient.GetDatabase(mongoConfig.DatabaseName);
            }
        }
        public IMongoCollection<T> MongoGetCollection<T>()
        {
            if (mongoDB == null)
                throw new ExceptionMongoDBNotInitialization();
            return mongoDB.GetCollection<T>(typeof(T).Name);
        }

        public BaseResponse<T> CreateResponse<T>() where T : new()
        {
            BaseResponse<T> response = new BaseResponse<T>
            {
                OK = false,
                Data = new T(),
                List = new List<T>()
            };

            return response;
        }
        public void ExceptionHandle(IBaseResponse response, Exception e)
        {
            response.OK = false;
            response.Message = e.Message;
            response.Code = e.GetType().Name;
        }
        private string GetKey(string tableName, string fieldName)
        {
            return tableName + "." + fieldName;
        }
        public bool IsExist(string tableName, string fieldName)
        {
            string key = GetKey(tableName, fieldName);
            return cacheDB.GetString(key) != null;
        }
        public async Task<bool> IsExistAsync(string tableName, string fieldName)
        {
            string key = GetKey(tableName, fieldName);
            return await cacheDB.GetStringAsync(key) != null;
        }
        public bool IsEqual(string tableName, string fieldName, string value)
        {
            string key = GetKey(tableName, fieldName);
            if (value == null)
            {
                return false;
            }
            else return value.Equals(cacheDB.GetString(key));
        }
        public async Task<bool> IsEqualAsync(string tableName, string fieldName, string value)
        {
            string key = GetKey(tableName, fieldName);
            if (value == null)
            {
                return false;
            }
            else
            {
                string data = await cacheDB.GetStringAsync(key);
                return value.Equals(data);
            };
        }
        public void Set(string tableName, string fieldName, string value)
        {
            string key = GetKey(tableName, fieldName);
            cacheDB.SetStringAsync(key, value);
        }
        public void SetWithExpire(string tableName, string fieldName, string value, int expiredAfterSecond)
        {
            var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(expiredAfterSecond));
            string key = GetKey(tableName, fieldName);
            cacheDB.SetStringAsync(key, value, options);
        }
        public string Get(string tableName, string fieldName)
        {
            string key = GetKey(tableName, fieldName);
            return cacheDB.GetString(key);
        }
        public async Task<string> GetAsync(string tableName, string fieldName)
        {
            string key = GetKey(tableName, fieldName);
            return await cacheDB.GetStringAsync(key);
        }
        public void Remove(string tableName, string fieldName)
        {
            string key = GetKey(tableName, fieldName);
            cacheDB.RemoveAsync(key);
        }
        public List<T> GetList<T>(string tableName, string fieldName)
        {
            string key = GetKey(tableName, fieldName);
            string value = cacheDB.GetString(key);
            List<T> list = JsonHelper.JsonToObject<List<T>>(value);
            if (list == null)
                list = new List<T>();
            return list;
        }
        public async Task<List<T>> GetListAsync<T>(string tableName, string fieldName)
        {
            string key = GetKey(tableName, fieldName);
            string value = await cacheDB.GetStringAsync(key);
            List<T> list = JsonHelper.JsonToObject<List<T>>(value);
            return list;
        }
        public void SetList<T>(string tableName, string fieldName, List<T> list)
        {
            string key = GetKey(tableName, fieldName);
            string value = JsonHelper.ObjectToJson(list);
            _ = cacheDB.SetStringAsync(key, value);
        }
        public T GetObject<T>(string tableName, string fieldName) where T : new()
        {
            string key = GetKey(tableName, fieldName);
            string json = cacheDB.GetString(key);
            return JsonHelper.JsonToObject<T>(json);
        }
        public async Task<T> GetObjectAsync<T>(string tableName, string fieldName) where T : new()
        {
            string key = GetKey(tableName, fieldName);
            string json = await cacheDB.GetStringAsync(key);
            return JsonHelper.JsonToObject<T>(json);
        }
        public void SetObject<T>(string tableName, string fieldName, T data)
        {
            string key = GetKey(tableName, fieldName);
            string json = JsonHelper.ObjectToJson(data);
            cacheDB.SetStringAsync(key, json);
        }
    }
}
