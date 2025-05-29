using ApiWallet.Models.DTos;

namespace ApiWallet.Services.Interfaces
{
    public interface IUserPesosBalanceService
    {
        Task<UserPesosBalanceDto> GetBalanceAsync(int userId);
        Task<bool> AddBalanceAsync(int userId, decimal amount);
        Task<bool> SubtractBalanceAsync(int userId, decimal amount);
    }
}
