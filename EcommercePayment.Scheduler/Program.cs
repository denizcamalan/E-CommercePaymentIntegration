using EcommercePayment.Scheduler;
using ECommercePayment.Application.Services.Abstaract;
using ECommercePayment.Application.Services.Concreate;
using ECommercePayment.Domain.AppSettings;
using ECommercePayment.Infrastructure;
using ECommercePayment.Infrastructure.Cache;
using ECommercePayment.Infrastructure.UOW;
using ECommercePayment.Integrations.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Serilog;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.File("logs/job/scheduler.log", rollingInterval: RollingInterval.Day).Enrich.FromLogContext()
    .CreateLogger();

builder.Services.AddWindowsService();
builder.Services.AddHostedService<UserSyncWorker>();

var configuration = builder.Configuration;

builder.Services.AddHttpClient();

builder.Services.AddTransient<IBalanceManagementService, BalanceManagementService>();
builder.Services.AddTransient<IProductService, ProductService>();
builder.Services.AddTransient<IOrderService, OrderService>();
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

var host = builder.Build(); 
host.Run();
