using CacheManager.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Ocelot config hot reload
builder.Configuration
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// MVC (internal rollback controller için)
builder.Services.AddControllers();

// JWT
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("GatewayJwt", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = "saas-platform",
            ValidAudience = "saas-platform-clients",
            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes("SUPER_SECRET_KEY_12345"))
        };
    });
// CacheManager (Ocelot rate limit / QoS için)
builder.Services.AddCacheManager<object>(settings => { settings.WithRedisConfiguration("redis", redis => { redis.WithEndpoint(builder.Configuration["Redis:ConnectionString"], 6379); }); settings.WithRedisCacheHandle("redis"); settings.WithMaxRetries(100).WithRetryTimeout(50); });
// Ocelot + Polly
builder.Configuration
    .AddJsonFile(
        "ocelot.json",
        optional: false,
        reloadOnChange: true
    );
builder.Services
    .AddOcelot(builder.Configuration)
    .AddPolly();

var app = builder.Build();

// Auth pipeline
app.UseAuthentication();
app.UseAuthorization();

// INTERNAL CONTROLLERS (rollback webhook vs)
app.MapControllers()
   .RequireAuthorization(policy =>
       policy.RequireAssertion(context =>
           context.User.Identity?.IsAuthenticated == false &&
           context.Resource is HttpContext http &&
           http.Request.Headers["X-Internal-Secret"] ==
               builder.Configuration["Internal:Secret"]
       ));

// Gateway
await app.UseOcelot();

app.Run();
