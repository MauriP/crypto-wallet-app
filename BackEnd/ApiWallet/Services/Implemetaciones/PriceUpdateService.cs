using ApiWallet.Data;
using ApiWallet.Models.Entyties;
using ApiWallet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApiWallet.Services.Implemetaciones
{
    public class PriceUpdaterService
    {
        private readonly WalletDbContext _context;
        private readonly ICryptoYaApiCliente _api;

        public PriceUpdaterService(WalletDbContext context, ICryptoYaApiCliente api)
        {
            _context = context;
            _api = api;
        }

        public async Task UpdatePricesAsync(string cryptoCode)
        {
            var exchanges = new[] { "satoshitango", "buenbit", "binance" };

            foreach (var exc in exchanges)
            {
                try
                {
                    var buy = await _api.GetBuyPriceAsync(cryptoCode, exc);
                    var sell = await _api.GetSellPriceAsync(cryptoCode, exc);

                    var cryptoId = await _context.Cryptocurrencies
                        .Where(c => c.Code == cryptoCode)
                        .Select(c => c.Id)
                        .FirstAsync();

                    var exchangeId = await _context.Exchanges
                        .Where(e => e.Code == exc)
                        .Select(e => e.Id)
                        .FirstAsync();

                    var existingPrice = await _context.CryptoPrices
                        .FirstOrDefaultAsync(p =>
                            p.CryptoId == cryptoId &&
                            p.ExchangeId == exchangeId);

                    if (existingPrice != null)
                    {
                        // Hacemos un update
                        existingPrice.BuyPrice = buy;
                        existingPrice.SellPrice = sell;
                        existingPrice.LastUpdated = DateTime.UtcNow;

                        _context.CryptoPrices.Update(existingPrice);
                    }
                    else
                    {
                        // Insert nuevo
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
                    Console.WriteLine($"Error actualizando precio para {cryptoCode} en {ex}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
        }
    }

}
