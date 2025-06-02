using ApiWallet.Models.DTos;

namespace ApiWallet.Services.Interfaces
{
    public interface IBalanceService
    {
        Task<UserTotalBalanceDto> GetUserBalanceAsync(int userId);
    }
}
