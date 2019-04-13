using Microsoft.Extensions.Options;
using RuPengMessageHub.Server.Settings;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RuPengMessageHub.Server.Helpers
{
    public class RedisHelper
    {
        private readonly IOptions<RedisSetting> redisSetting;
        private readonly ConnectionMultiplexer connectionMultiplexer;
        public RedisHelper(IOptions<RedisSetting> redisSetting)
        {
            this.redisSetting = redisSetting;
            connectionMultiplexer = ConnectionMultiplexer.Connect(redisSetting.Value.Configuration);
        }

        public IDatabase GetDatabase()
        {
            return connectionMultiplexer.GetDatabase(redisSetting.Value.DbId);
        }
    }
}
