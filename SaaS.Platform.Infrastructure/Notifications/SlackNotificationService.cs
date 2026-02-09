using Microsoft.Extensions.Configuration;
using SaaS.Platform.Application.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SaaS.Platform.Infrastructure.Notifications
{
    public sealed class SlackNotificationService : ISlackNotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public SlackNotificationService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task SendAsync(string message)
        {
            var payload = new
            {
                text = message,
                channel = _configuration["Slack:Channel"],
                username = _configuration["Slack:Username"]
            };

            var webhookUrl = _configuration["Slack:WebhookUrl"];

            await _httpClient.PostAsJsonAsync(webhookUrl, payload);
        }
    }
}
