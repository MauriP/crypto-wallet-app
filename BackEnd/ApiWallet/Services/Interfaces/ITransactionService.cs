using ApiWallet.Models.DTos;
using ApiWallet.Models.Entyties;

namespace ApiWallet.Services.Interfaces
{
    public interface ITransactionService
    {
        public Task<TransactionCreateDto> CreateTransactionAsync(TransactionRequestDto transactionDto);
        Task<IEnumerable<BestPriceDto>> GetBestPricesAsync(string cryptoCode, string action);
        Task<IEnumerable<WalletStatusDto>> GetWalletStatusAsync(int userId);
        Task<decimal> GetCryptoBalance(int userId, string cryptoCode);

    }
}
