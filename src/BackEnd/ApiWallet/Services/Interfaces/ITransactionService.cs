using ApiWallet.Models.DTos;
using ApiWallet.Models.Entyties;

namespace ApiWallet.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<TransactionCreateDto> CreateTransactionAsync(TransactionRequestDto transactionDto);
        Task<IEnumerable<BestPriceDto>> GetBestPricesAsync(string cryptoCode, string action);
        Task<IEnumerable<WalletStatusDto>> GetWalletStatusAsync(int userId);
        Task<IEnumerable<TransactionCreateDto>> GetUserTransactionsAsync(int userId);
        Task<decimal> GetCryptoBalance(int userId, string cryptoCode);
        Task<TransactionCreateDto> GetTransactionByIdAsync(int id);

        // Add missing method signatures
        Task<decimal?> GetBuyPriceAsync(string cryptoCode, string exchangeCode);
        Task<decimal?> GetSellPriceAsync(string cryptoCode, string exchangeCode);
    }
}
