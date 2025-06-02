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

        public async Task UpdatePricesAsync(string cryptoCode)
        {
            var exchanges = new[] { "satoshitango", "buenbit", "binance" };
            foreach (var exc in exchanges)
            {
                _logger.LogInformation("Iniciando actualización de precios para {Crypto} en {Exchange}", cryptoCode, exc);
                await UpdateExchangePrice(cryptoCode, exc);
            }
            _logger.LogInformation("Guardando cambios en la base de datos para {Crypto}", cryptoCode);
            await _context.SaveChangesAsync();
        }

        private async Task UpdateExchangePrice(string cryptoCode, string exchangeCode)
        {
            try
            {
                _logger.LogInformation("Obteniendo precios desde API o base de datos para {Crypto} en {Exchange}", cryptoCode, exchangeCode);
                var (buy, sell) = await GetPricesFromApiOrDatabase(cryptoCode, exchangeCode);

                _logger.LogInformation("Obteniendo ID de la criptomoneda {Crypto}", cryptoCode);
                var cryptoId = await _context.Cryptocurrencies
                    .Where(c => c.Code == cryptoCode)
                    .Select(c => c.Id)
                    .FirstAsync();

                _logger.LogInformation("Obteniendo ID del exchange {Exchange}", exchangeCode);
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
                    _logger.LogInformation("Actualizando precio existente para {Crypto} en {Exchange}: Buy={Buy}, Sell={Sell}", cryptoCode, exchangeCode, buy, sell);
                    existingPrice.BuyPrice = buy;
                    existingPrice.SellPrice = sell;
                    existingPrice.LastUpdated = DateTime.UtcNow;
                    _context.CryptoPrices.Update(existingPrice);
                }
                else
                {
                    _logger.LogInformation("Insertando nuevo precio para {Crypto} en {Exchange}: Buy={Buy}, Sell={Sell}", cryptoCode, exchangeCode, buy, sell);
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

        // Implementación básica para evitar error de compilación
        private async Task<(decimal? buy, decimal? sell)> GetPricesFromApiOrDatabase(string cryptoCode, string exchangeCode)
        {
            _logger.LogInformation("Llamando a la API para obtener precio de compra de {Crypto} en {Exchange}", cryptoCode, exchangeCode);
            var buy = await _api.GetBuyPriceAsync(cryptoCode, exchangeCode);
            _logger.LogInformation("Llamando a la API para obtener precio de venta de {Crypto} en {Exchange}", cryptoCode, exchangeCode);
            var sell = await _api.GetSellPriceAsync(cryptoCode, exchangeCode);
            _logger.LogInformation("Precios obtenidos para {Crypto} en {Exchange}: Buy={Buy}, Sell={Sell}", cryptoCode, exchangeCode, buy, sell);
            return (buy, sell);
        }
    }
}