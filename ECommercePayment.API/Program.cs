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
using ECommercePayment.Application.Services.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using ECommercePayment.Domain.DTOs.Response;
using ECommercePayment.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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


builder.Services.AddControllers().AddJsonOptions(x =>
{
    x.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
}).ConfigureApiBehaviorOptions(opt =>
{
    //Custom Response Message
    opt.InvalidModelStateResponseFactory = context =>
    {
        var InvalidKeys = context.ModelState.Where(s => s.Value.ValidationState.HasFlag(Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)).Select(d => d.Key).ToList();

        BaseErrorResponse response = new BaseErrorResponse()
        {
            title = "Validation Failed",
            path = string.Join(", ", InvalidKeys),
            message = "One or more validation errors occurred.",
            httpCode = System.Net.HttpStatusCode.BadRequest,
            errorCode = ErrorCodes.ValidationFailed,
            timestamp = DateTime.UtcNow
        };

        return new UnprocessableEntityObjectResult(response)
        {
            ContentTypes = { "application/json" },
            StatusCode = (int)HttpStatusCode.BadRequest,
        };
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "E-Commerce Payment API",
        Version = "v1",
        Description = "Backend for e-commerce platform integrating with Balance Management service."
    });

    // Swagger'a Bearer Authentication ekle
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddHttpClient();

builder.Services.AddScoped<IBalanceManagementService, BalanceManagementService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IECommerceIdentityService, ECommerceIdentityService>();
builder.Services.AddScoped<IUOW, UOW>();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

BalanceManagementSettings bMSettings = new BalanceManagementSettings();
configuration.GetSection("BalanceManagement").Bind(bMSettings);

builder.Services.AddSingleton<BalanceManagementSettings>(bMSettings);

// JWT Settings
JwtSettings jwtSettings = new JwtSettings();
configuration.GetSection("JwtSettings").Bind(jwtSettings);
builder.Services.AddSingleton<JwtSettings>(jwtSettings);

// JWT Authentication
var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Production'da true olmalı
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
