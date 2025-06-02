using ApiWallet.Models.DTos;
using ApiWallet.Services.Interfaces;
using System.Text.Json;

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

        // Obtiene el precio de compra de una criptomoneda en un exchange específico
        public async Task<decimal?> GetBuyPriceAsync(string cryptoCode, string exchangeCode)
        {
            try
            {
                var url = $"{exchangeCode.ToLower()}/{cryptoCode.ToUpper()}/ARS/1";
                var response = await _httpClient.GetAsync(url);

                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Respuesta no exitosa de CriptoYa: {StatusCode} - {Content}", response.StatusCode, content);
                    return null;
                }

                if (!response.Content.Headers.ContentType?.MediaType.Contains("json") ?? true)
                {
                    _logger.LogError("Respuesta no es JSON: {Content}", content);
                    return null;
                }

                var dict = JsonSerializer.Deserialize<Dictionary<string, decimal>>(content);
                return dict != null && dict.ContainsKey("ask") ? dict["ask"] : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener precio de compra para {Crypto} en {Exchange}", cryptoCode, exchangeCode);
                throw;
            }
        }

        // Obtiene el precio de venta de una criptomoneda en un exchange específico
        public async Task<decimal?> GetSellPriceAsync(string cryptoCode, string exchangeCode)
        {
            try
            {
                var url = $"{exchangeCode.ToLower()}/{cryptoCode.ToUpper()}/ARS/1";
                var response = await _httpClient.GetAsync(url);

                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Respuesta no exitosa de CriptoYa: {StatusCode} - {Content}", response.StatusCode, content);
                    return null;
                }

                // Intenta deserializar solo si el content-type es JSON
                if (!response.Content.Headers.ContentType?.MediaType.Contains("json") ?? true)
                {
                    _logger.LogError("Respuesta no es JSON: {Content}", content);
                    return null;
                }

                var dict = JsonSerializer.Deserialize<Dictionary<string, decimal>>(content);
                // El precio de venta suele ser el precio "bid"
                return dict != null && dict.ContainsKey("bid") ? dict["bid"] : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener precio de venta para {Crypto} en {Exchange}", cryptoCode, exchangeCode);
                throw;
            }
        }

        // Obtiene los precios de compra y venta de todas las exchanges soportadas para una criptomoneda específica
        public async Task<IEnumerable<BestPriceDto>> GetAllPrices(string cryptoCode)
        {
            var supportedExchanges = new[] { "satoshitango", "buenbit", "binance" };
            var results = new List<BestPriceDto>();

            foreach (var ex in supportedExchanges)
            {
                try
                {
                    var response = await _httpClient.GetFromJsonAsync<Dictionary<string, decimal>>($"{ex}/{cryptoCode}/ars");
                    decimal? price = null;
                    if (response != null && response.ContainsKey("ask"))
                        price = response["ask"];

                    results.Add(new BestPriceDto
                    {
                        ExchangeCode = ex,
                        Price = price ?? 0, // Use null-coalescing operator to handle null
                        LastUpdate = DateTime.UtcNow
                    });
                }
                catch (Exception exErr)
                {
                    // Si falla, igual agrego el exchange con precio 0
                    results.Add(new BestPriceDto
                    {
                        ExchangeCode = ex,
                        Price = 0, // Cambiado de null a 0 para evitar el error CS0037
                        LastUpdate = DateTime.UtcNow
                    });
                    _logger.LogWarning(exErr, $"No se pudo obtener precio para {cryptoCode} en {ex}");
                }
            }
            return results;
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
