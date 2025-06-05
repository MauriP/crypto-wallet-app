using ApiWallet.Data;
using ApiWallet.Models.DTos;
using ApiWallet.Models.Entyties;
using ApiWallet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApiWallet.Services.Implemetaciones
{
    public class UserPesosBalanceService : IUserPesosBalanceService
    {
        private readonly WalletDbContext _context;

        public UserPesosBalanceService(WalletDbContext context)
        {
            _context = context;
        }

        // Obtiene el saldo de un usuario, sumando o restando según las transacciones
        public async Task<UserPesosBalanceDto> GetBalanceAsync(int userId)
        {
            // Obtener el saldo base guardado (si existe)
            var baseBalance = await _context.UserPesosBalances
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.UserId == userId);

            decimal saldo = baseBalance?.PesosBalance ?? 0;

            // Sumar o restar según las transacciones
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId && (t.Action == "purchase" || t.Action == "sale"))
                .ToListAsync();

            foreach (var t in transactions)
            {
                if (t.Action == "sale")
                    saldo += t.Money;
                else if (t.Action == "purchase")
                    saldo -= t.Money;
            }

            return new UserPesosBalanceDto
            {
                UserId = userId,
                PesosBalance = saldo
            };
        }

        // Agrega o resta saldo del usuario, creando un nuevo registro si no existe
        public async Task<bool> AddBalanceAsync(int userId, decimal amount)
        {
            var balance = await _context.Set<UserPesosBalance>().FirstOrDefaultAsync(b => b.UserId == userId);
            if (balance == null)
            {
                balance = new UserPesosBalance { UserId = userId, PesosBalance = amount };
                _context.Add(balance);
            }
            else
            {
                balance.PesosBalance += amount;
                _context.Update(balance);
            }
            await _context.SaveChangesAsync();
            return true;
        }

        // Para en un futuro, sirve para realizar una extracción de saldo del usuario
        public async Task<bool> SubtractBalanceAsync(int userId, decimal amount)
        {
            var balance = await _context.Set<UserPesosBalance>().FirstOrDefaultAsync(b => b.UserId == userId);
            if (balance == null || balance.PesosBalance < amount)
                return false;
            balance.PesosBalance -= amount;
            _context.Update(balance);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
