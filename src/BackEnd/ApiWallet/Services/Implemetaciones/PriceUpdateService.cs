using ApiWallet.Data;
using ApiWallet.Models.Entyties;
using ApiWallet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ApiWallet.Services.Implemetaciones
{
    public class PriceUpdaterService
    {
        private readonly WalletDbContext _context;
        private readonly ICryptoYaApiCliente _api;
        private readonly ILogger<PriceUpdaterService> _logger;

        public PriceUpdaterService(WalletDbContext context, ICryptoYaApiCliente api, ILogger<PriceUpdaterService> logger)
        {
            _context = context;
            _api = api;
            _logger = logger;
        }

        // Actualiza los precios de las criptomonedas en la base de datos
        public async Task UpdatePricesAsync(string cryptoCode)
        {
            var exchanges = new[] { "satoshitango", "buenbit", "binance" };
            foreach (var exc in exchanges)
            {
                
                await UpdateExchangePrice(cryptoCode, exc);
            }
            await _context.SaveChangesAsync();
        }

        // Actualiza el precio de una criptomoneda en un exchange específico
        private async Task UpdateExchangePrice(string cryptoCode, string exchangeCode)
        {
            try
            {
                 var (buy, sell) = await GetPricesFromApiOrDatabase(cryptoCode, exchangeCode);

                var cryptoId = await _context.Cryptocurrencies
                    .Where(c => c.Code == cryptoCode)
                    .Select(c => c.Id)
                    .FirstAsync();

                var exchangeId = await _context.Exchanges
                    .Where(e => e.Code == exchangeCode)
                    .Select(e => e.Id)
                    .FirstAsync();

                _logger.LogInformation("Buscando precio existente para {Crypto} en {Exchange}", cryptoCode, exchangeCode);
                var existingPrice = await _context.CryptoPrices
                    .FirstOrDefaultAsync(p =>
                        p.CryptoId == cryptoId &&
                        p.ExchangeId == exchangeId);

                if (existingPrice != null)
                {
                    
                    existingPrice.BuyPrice = buy;
                    existingPrice.SellPrice = sell;
                    existingPrice.LastUpdated = DateTime.UtcNow;
                    _context.CryptoPrices.Update(existingPrice);
                }
                else
                {
                     var price = new CryptoPrice
                    {
                        CryptoId = cryptoId,
                        ExchangeId = exchangeId,
                        BuyPrice = buy,
                        SellPrice = sell,
                        LastUpdated = DateTime.UtcNow
                    };
                    _context.CryptoPrices.Add(price);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando precio para {Crypto} en {Exchange}", cryptoCode, exchangeCode);
            }
        }

        // Obtiene los precios de compra y venta desde la API o la base de datos
        private async Task<(decimal? buy, decimal? sell)> GetPricesFromApiOrDatabase(string cryptoCode, string exchangeCode)
        {

            var buy = await _api.GetBuyPriceAsync(cryptoCode, exchangeCode);
            var sell = await _api.GetSellPriceAsync(cryptoCode, exchangeCode);
            _logger.LogInformation("Precios obtenidos para {Crypto} en {Exchange}: Buy={Buy}, Sell={Sell}", cryptoCode, exchangeCode, buy, sell);
            return (buy, sell);
        }
    }
}