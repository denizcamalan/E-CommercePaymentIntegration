using ECommercePayment.Domain.Consts;
using ECommercePayment.Domain.DTOs.Response;
using ECommercePayment.Infrastructure.Cache;
using ECommercePayment.Integrations.BalanceManagement.Models.Response.Balance;
using ECommercePayment.Integrations.Services;

namespace EcommercePayment.Scheduler;

public class UserSyncWorker : BackgroundService
{
    private readonly ILogger<UserSyncWorker> _logger;
    private readonly ICacheService _cacheService;
    private readonly IBalanceManagementService _balanceManagementService;

    public UserSyncWorker(ILogger<UserSyncWorker> logger, ICacheService cacheService, IBalanceManagementService balanceManagementService)
    {
        _logger = logger;
        _cacheService = cacheService;
        _balanceManagementService = balanceManagementService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            
        var response = new BaseResponse<UserBalanceResponse> { Data = new UserBalanceResponse() };

            var externalResponse = await _balanceManagementService.GetUserBalance();

            if (externalResponse.Success.HasValue && externalResponse.Data is not null)
            {
                await _cacheService.SetAsync(CacheKeys.UserInfo, externalResponse.Data, TimeSpan.FromDays(1));

                response.Data = externalResponse.Data;

                _logger.LogInformation("User balance updated in cache.");
            }

            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
}
