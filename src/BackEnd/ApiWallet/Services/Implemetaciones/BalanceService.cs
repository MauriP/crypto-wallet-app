using ApiWallet.Data;
using ApiWallet.Models.DTos;
using ApiWallet.Models.Entyties;
using ApiWallet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApiWallet.Services.Implemetaciones
{
    public class BalanceService : IBalanceService
    {
        private readonly WalletDbContext _context;
        public BalanceService(WalletDbContext context)
        {
            _context = context;
        }
        public async Task<UserTotalBalanceDto> GetUserBalanceAsync(int userId)
        {
            var results = await _context
                .Set<VWalletSummary>()  // Esta entidad la definimos más abajo
                .Where(ws => ws.UserId == userId)
                .ToListAsync();

            var cryptoBalances = results.Select(r => new CryptoBalanceDetailDto
            {
                CryptoCode = r.CryptoCode,
                CryptoAmount = r.TotalAmount,
                CurrentPrice = r.CurrentPrice,
                ValueInARS = r.CurrentValue
            }).ToList();

            var totalBalance = cryptoBalances.Sum(cb => cb.ValueInARS);

            return new UserTotalBalanceDto
            {
                UserId = userId,
                TotalBalanceInARS = totalBalance,
                CryptoBalances = cryptoBalances
            };
        }
    }
}
