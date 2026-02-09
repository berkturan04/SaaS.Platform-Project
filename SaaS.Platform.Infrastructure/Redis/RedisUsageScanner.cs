using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaaS.Platform.Infrastructure.Redis
{
    public sealed class RedisUsageScanner
    {
        private readonly IServer _server;
        private readonly IDatabase _db;

        public RedisUsageScanner()
        {
            var mux = RedisConnection.Instance;
            _server = mux.GetServer(mux.GetEndPoints().First());
            _db = mux.GetDatabase();
        }

        // Redis SCAN komutu ile tüm usage key'lerini çekiyoruz
        public async Task<List<(Guid tenantId, int usage)>> ScanAsync(int year, int month)
        {
            var pattern = $"usage:*:{year}{month:00}";  // Key pattern
            var result = new List<(Guid, int)>();

            // SCAN komutu kullanılarak Redis'teki usage key'lerini alıyoruz
            await foreach (var key in _server.KeysAsync(pattern: pattern))
            {
                var value = await _db.StringGetAsync(key);
                if (!value.HasValue) continue;

                var parts = key.ToString().Split(':');
                var tenantId = Guid.Parse(parts[1]);

                result.Add((tenantId, (int)value));
            }

            return result;
        }
    }
}
