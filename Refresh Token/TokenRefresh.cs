using Microsoft.AspNetCore.Identity;

namespace Refresh_Token
{
    public class TokenRefresh : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TokenRefresh> _logger;

        public TokenRefresh(IServiceProvider serviceProvider, ILogger<TokenRefresh> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(true)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<TokenDatabase>();
                    var refreshTokenContext = scope.ServiceProvider
                        .GetRequiredService<RefreshTokenContext>();
                    var tokens = db.Record;
                    
                    foreach(var (customId, tokenInfo) in tokens)
                    {
                        if(tokenInfo.Expires.Subtract(DateTime.UtcNow) < TimeSpan.FromDays(1))
                        {
                            _logger.LogInformation($"Refreshing token for {customId}");

                            var result = await refreshTokenContext
                                .RefreshTokenAsync(tokenInfo, stoppingToken);
                            db.Save(customId, new TokenInfo(
                                result.AccessToken,
                                result.RefreshToken,
                                DateTime.UtcNow.AddSeconds(int.Parse(result.ExpiresIn)
                                )
                            ));
                        }
                    }
                }
                catch
                {
                    throw;
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
