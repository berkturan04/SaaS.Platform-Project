using Microsoft.EntityFrameworkCore;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Prometheus;
using SaaS.Platform.API.Middlewares;
using SaaS.Platform.API.Swagger;
using SaaS.Platform.Application.Interfaces;
using SaaS.Platform.Application.Notifications;
using SaaS.Platform.Application.Payments;
using SaaS.Platform.Application.RateLimiting;
using SaaS.Platform.Application.Usage;
using SaaS.Platform.Infrastructure.BackgroundJobs;
using SaaS.Platform.Infrastructure.Caching;
using SaaS.Platform.Infrastructure.Notifications;
using SaaS.Platform.Infrastructure.Payments;
using SaaS.Platform.Infrastructure.Persistance;
using SaaS.Platform.Infrastructure.Usage;
using SaaS.Platform.Shared.Tenant;
using StackExchange.Redis;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect("localhost:6379"));


//Slack
builder.Services.AddHttpClient();

builder.Services.AddScoped<ISlackNotificationService, SlackNotificationService>();

// Webhook + retry
builder.Services.AddHostedService<WebhookProcessor>();
builder.Services.AddHostedService<PaymentProcessorJob>();
builder.Services.AddScoped<IPaymentGateway, FakePaymentGateway>();

// Billing to payment
builder.Services.AddScoped<IPaymentProvider,
    FakePaymentProvider>();

// Redis Quota
builder.Services.AddScoped<IQuotaService, RedisQuotaService>();

// Ocelot
builder.Services.AddOcelot();

// Idempotent Billing Job
builder.Services.AddHostedService<MonthlyBillingJob>();

// Tenant Rate Limiter
builder.Services.AddSingleton<ITenantRateLimiter, InMemoryTenantRateLimiter>();

//Usage Batching
builder.Services.AddSingleton<IUsageCollector, InMemoryUsageCollector>();
builder.Services.AddHostedService<UsageFlushJob>();

// Tenant Provider
builder.Services.AddScoped<ITenantProvider, TenantProvider>();

// AppDbContext
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Outbox Message
builder.Services.AddHostedService<OutboxProcessor>();

// Redis cache setup
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379"; // Redis sunucu adresi
    options.InstanceName = "SaaS_Platform_";  // Instance adý
});

// Usage Tracking Service registration
builder.Services.AddSingleton<IUsageTrackingService, UsageTrackingService>();


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<TenantHeaderOperationFilter>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseHttpMetrics();
app.UseMiddleware<TenantMetricsMiddleware>();

app.UseAuthorization();

app.UseMiddleware<TenantMiddleware>();
app.UseMiddleware<CanaryMetricsMiddleware>();

app.MapControllers();

app.MapMetrics();

app.UseOcelot().Wait();

app.Run();
