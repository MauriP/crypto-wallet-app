using ApiWallet.Models.DTos;
using ApiWallet.Services.Interfaces;

namespace ApiWallet.Services.Implemetaciones
{
    public class CryptoYaApiClient : ICryptoYaApiCliente
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CryptoYaApiClient> _logger;

        public CryptoYaApiClient(HttpClient httpClient, ILogger<CryptoYaApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpClient.BaseAddress = new Uri("https://criptoya.com/api/");
        }

        public async Task<decimal> GetBuyPriceAsync(string cryptoCode, string exchangeCode)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<Dictionary<string, decimal>>($"{exchangeCode}/{cryptoCode}/ars");
                return response["ask"];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener precio de compra para {cryptoCode} en {exchangeCode}");
                throw;
            }
        }

        public async Task<decimal> GetSellPriceAsync(string cryptoCode, string exchangeCode)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<Dictionary<string, decimal>>($"{exchangeCode}/{cryptoCode}/ars");
                return response["bid"];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener precio de venta para {cryptoCode} en {exchangeCode}");
                throw;
            }
        }

        public async Task<IEnumerable<BestPriceDto>> GetAllPrices(string cryptoCode)
        {
            try
            {
                var supportedExchanges = new[] { "satoshitango", "buenbit", "binance" };
                var tasks = supportedExchanges.Select(ex => GetExchangePrice(ex, cryptoCode));
                var results = await Task.WhenAll(tasks);

                return results.Where(r => r != null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener todos los precios para {cryptoCode}");
                throw;
            }
        }

        private async Task<BestPriceDto> GetExchangePrice(string exchangeCode, string cryptoCode)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<Dictionary<string, decimal>>($"{exchangeCode}/{cryptoCode}/usd");

                return new BestPriceDto
                {
                    ExchangeCode = exchangeCode,
                    Price = (response["totalBid"] + response["totalAsk"]) / 2,
                    LastUpdate = DateTime.UtcNow
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
