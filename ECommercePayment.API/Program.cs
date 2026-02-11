using ECommercePayment.Domain.AppSettings;
using ECommercePayment.Infrastructure.Cache;
using StackExchange.Redis;
using Serilog;
using ECommercePayment.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using ECommercePayment.Integrations.Services;
using ECommercePayment.Application.Services.Concreate;
using ECommercePayment.Application.Services.Abstaract;
using ECommercePayment.Infrastructure.UOW;

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

builder.Services.AddScoped<IBalanceManagementService, BalanceManagementService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUOW, UOW>();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

BalanceManagementSettings bMSettings = new BalanceManagementSettings();
configuration.GetSection("BalanceManagement").Bind(bMSettings);

builder.Services.AddSingleton<BalanceManagementSettings>(bMSettings);

string mainDbConnStr = configuration.GetConnectionString("PostgreSqlConnection") ?? throw new ArgumentNullException("ECommercePayment not found!");

Console.WriteLine("Connection string: " + mainDbConnStr);

builder.Services.AddDbContext<EcommercePaymentDbContext>(options =>
{
    options.UseNpgsql(mainDbConnStr, npgsqlOptions =>
    {
        npgsqlOptions.MigrationsAssembly(typeof(EcommercePaymentDbContext).Assembly.FullName);
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null);
        npgsqlOptions.MigrationsHistoryTable(tableName: HistoryRepository.DefaultTableName, schema: "OrderTable");
    });
});

// Redis connection & cache service
var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
builder.Services.AddSingleton<ICacheService, RedisCacheService>();

builder.Services.AddHealthChecks();

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
