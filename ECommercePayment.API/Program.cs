using ECommercePayment.Domain.AppSettings;
using ECommercePayment.Infrastructure.Cache;
using StackExchange.Redis;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
Console.WriteLine("Enviroment : " + builder.Environment.EnvironmentName);
var configuration = builder.Configuration;

var logger = new LoggerConfiguration()
                 .ReadFrom.Configuration(configuration)
                 .Enrich.FromLogContext()
                 .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "E-Commerce Payment API",
        Version = "v1",
        Description = "Backend for e-commerce platform integrating with Balance Management service."
    });
});

builder.Services.AddHttpClient();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

BalanceManagementSettings bMSettings = new BalanceManagementSettings();
configuration.GetSection("BalanceManagement").Bind(bMSettings);

builder.Services.AddSingleton<BalanceManagementSettings>(bMSettings);

// Redis connection & cache service
var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
builder.Services.AddSingleton<ICacheService, RedisCacheService>();

var app = builder.Build();

app.UseHealthChecks("/health");

app.UseExceptionHandler(_ => { });
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
