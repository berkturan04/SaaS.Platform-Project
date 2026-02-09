using Microsoft.AspNetCore.Mvc;
using SaaS.Platform.Application.Notifications;
using SaaS.Platform.Gateway.Services;

namespace SaaS.Platform.Gateway.Controllers.Internal
{
    [ApiController]
    [Route("internal/rollback")]
    public class CanaryRollbackController : ControllerBase
    {
        private readonly ILogger<CanaryRollbackController> _logger;
        private readonly ICanaryConfigService _canaryConfig;
        private readonly ISlackNotificationService _slack;
        private readonly IConfiguration _configuration;

        public CanaryRollbackController(
            ILogger<CanaryRollbackController> logger,
            ICanaryConfigService canaryConfig,
            ISlackNotificationService slack,
            IConfiguration configuration)
        {
            _logger = logger;
            _canaryConfig = canaryConfig;
            _slack = slack;
            _configuration = configuration;
        }

        [HttpPost("canary")]
        public async Task<IActionResult> RollbackCanary(
            [FromHeader(Name = "X-Internal-Secret")] string secret)
        {
            if (secret != _configuration["Internal:Secret"])
                return Unauthorized();

            _logger.LogCritical("CANARY ROLLBACK TRIGGERED");

            await _canaryConfig.DisableCanaryAsync();

            await _slack.SendAsync(
                "🚨 *CANARY ROLLBACK TRIGGERED*\n" +
                "• Reason: Error rate exceeded threshold\n" +
                $"• Time (UTC): {DateTime.UtcNow:O}"
            );

            return Ok(new
            {
                status = "rollback-triggered",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
