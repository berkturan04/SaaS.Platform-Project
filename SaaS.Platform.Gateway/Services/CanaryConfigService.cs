using System.Text.Json;

namespace SaaS.Platform.Gateway.Services
{
    public class CanaryConfigService : ICanaryConfigService
    {
        private readonly IConfiguration _configuration;

        public CanaryConfigService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task DisableCanaryAsync()
        {
            var ocelotPath = "ocelot.json";

            var json = await File.ReadAllTextAsync(ocelotPath);
            var doc = JsonDocument.Parse(json);

            var root = doc.RootElement.Clone();

            // ⚠️ Prod'da: strongly typed + atomic write
            // Şimdilik konseptual rollback
            // api-v2 weight = 0
            // api-v1 weight = 100

            // burada genelde:
            // - template json
            // - env var
            // - config service (Consul / etcd)

            File.Copy("ocelot.rollback.json", "ocelot.json", overwrite: true);
        }
    }
}
