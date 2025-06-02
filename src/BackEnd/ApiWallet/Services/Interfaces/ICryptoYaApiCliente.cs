
using ApiWallet.Models.DTos;

namespace ApiWallet.Services.Interfaces
{
    public interface ICryptoYaApiCliente
    {
        Task<decimal?> GetBuyPriceAsync(string cryptoCode, string exchangeCode);
        Task<decimal?> GetSellPriceAsync(string cryptoCode, string exchangeCode);
        Task<IEnumerable<BestPriceDto>> GetAllPrices(string cryptoCode);

    }
}
