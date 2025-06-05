namespace ApiWallet.Services.Implemetaciones
{
    public class PriceBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<PriceBackgroundService> _logger;

        public PriceBackgroundService(IServiceProvider services, ILogger<PriceBackgroundService> logger)
        {
            _services = services;
            _logger = logger;
        }

        // Me actualiza los precios de las crypto en la base de datos cada 30 segundos 
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _services.CreateScope())
                {
                    var priceUpdater = scope.ServiceProvider.GetRequiredService<PriceUpdaterService>();
                    try
                    {
                        await priceUpdater.UpdatePricesAsync("btc");
                        await priceUpdater.UpdatePricesAsync("eth");
                        await priceUpdater.UpdatePricesAsync("usdc");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating prices in background");
                    }
                }
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // Actualiza cada 30 segundos
            }
        }
    }
}
