using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SaaS.Platform.Gateway.Controllers.Internal
{
    [ApiController]
    [Route("internal/rollback")]
    public class InternalRollbackController : ControllerBase
    {
        private readonly IConfiguration _config;

        public InternalRollbackController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("canary")]
        public IActionResult Rollback([FromQuery] string secret)
        {
            if (secret != _config["Internal:Secret"])
                return Unauthorized();

            var path = Path.Combine(
                AppContext.BaseDirectory,
                "ocelot.json"
            );

            var json = System.IO.File.ReadAllText(path);
            var doc = JsonNode.Parse(json)!;

            var hosts = doc["Routes"]![0]!["DownstreamHostAndPorts"]!;

            hosts[0]!["Weight"] = 100; // api-v1
            hosts[1]!["Weight"] = 0;   // api-v2

            System.IO.File.WriteAllText(
                path,
                doc.ToJsonString(new JsonSerializerOptions
                {
                    WriteIndented = true
                })
            );

            return Ok("Canary rolled back");
        }
    }
}
