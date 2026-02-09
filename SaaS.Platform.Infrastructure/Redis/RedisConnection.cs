using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaaS.Platform.Infrastructure.Redis
{
    public sealed class RedisConnection
    {
        private static readonly Lazy<ConnectionMultiplexer> _lazy =
            new(() => ConnectionMultiplexer.Connect("localhost:6379"));

        public static ConnectionMultiplexer Instance => _lazy.Value;
    }
}
