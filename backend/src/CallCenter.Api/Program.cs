using System.Text;
using CallCenter.Api.Hubs;
using CallCenter.Application.Interfaces;
using CallCenter.Application.Services;
using CallCenter.Infrastructure.ExternalServices;
using CallCenter.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ---- Railway port binding ----
// Railway injects a PORT env var and routes traffic to it; Kestrel must listen on
// 0.0.0.0:$PORT rather than the localhost dev ports used in launchSettings.json.
var railwayPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(railwayPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{railwayPort}");
}

// ---- Configuration ----
var jwtSection = builder.Configuration.GetSection("Jwt");
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

// ---- Persistence ----
// SQLite for a realistic, file-backed demo. Swap to UseInMemoryDatabase("callcenter") for
// quick local runs with no file artifacts if preferred.
builder.Services.AddDbContext<CallCenterDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// ---- Dependency Injection: Repositories / Application seams ----
builder.Services.AddScoped<IAgentRepository, AgentRepository>();
builder.Services.AddScoped<IQueueRepository, QueueRepository>();
builder.Services.AddScoped<ICallRepository, CallRepository>();
builder.Services.AddScoped<ICrmService, MockCrmService>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IRoutingEngine, RoutingEngine>();
builder.Services.AddScoped<ICallNotifier, CallHubNotifier>();
builder.Services.AddScoped<CallOrchestrationService>();
builder.Services.AddScoped<ReportingService>();

// ---- Auth ----
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!))
    };

    // Allow the JWT to be read from the query string for SignalR's WebSocket handshake,
    // which can't set an Authorization header.
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/call"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// ---- CORS (Angular dev server) ----
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // required for SignalR
    });
});

// ---- API / SignalR / Swagger ----
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // The Angular client sends/expects enum values as strings (e.g. "Offline"),
        // not their underlying numeric values, so opt into the string converter.
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Call Center Platform Prototype API", Version = "v1" });
});

var app = builder.Build();

// ---- Seed demo data on startup ----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CallCenterDbContext>();
    db.Database.EnsureCreated();
    DbSeeder.Seed(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AngularClient");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<CallHub>("/hubs/call");

app.Run();
