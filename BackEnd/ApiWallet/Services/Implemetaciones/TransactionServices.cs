using ApiWallet.Data;
using ApiWallet.Models.DTos;
using ApiWallet.Models.Entyties;
using ApiWallet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApiWallet.Services.Implemetaciones
{
    public class TransactionService : ITransactionService
    {
        private readonly WalletDbContext _context;
        private readonly ICryptoYaApiCliente _cryptoYaApiClient;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(WalletDbContext context, ICryptoYaApiCliente cryptoYaApiClient, ILogger<TransactionService> logger)
        {
            _context = context;
            _cryptoYaApiClient = cryptoYaApiClient;
            _logger = logger;
        }

        public async Task<TransactionCreateDto> CreateTransactionAsync(TransactionRequestDto request)
        {
            if (request.CryptoAmount <= 0)
                throw new ArgumentException("El monto de la criptomoneda debe ser mayor que cero");

            var crypto = await _context.Cryptocurrencies.FirstOrDefaultAsync(c => c.Code == request.CryptoCode);
            if (crypto == null)
                throw new KeyNotFoundException("Criptomoneda no encontrada");

            var exchange = await _context.Exchanges.FirstOrDefaultAsync(e => e.Code == request.ExchangeCode);
            if (exchange == null)
                throw new KeyNotFoundException("Exchange no encontrado");

            decimal price;
            if (request.Action == "purchase")
            {
                price = await _cryptoYaApiClient.GetBuyPriceAsync(request.CryptoCode, request.ExchangeCode);
            }
            else if (request.Action == "sale")
            {
                var balance = await GetCryptoBalance(request.UserId, request.CryptoCode);
                if (balance < request.CryptoAmount)
                    throw new InvalidOperationException("Saldo insuficiente para realizar la venta");

                price = await _cryptoYaApiClient.GetSellPriceAsync(request.CryptoCode, request.ExchangeCode);
            }
            else
            {
                throw new ArgumentException("Acción no válida. Debe ser 'purchase' o 'sale'");
            }

            var money = request.CryptoAmount * price;

            var transaction = new Transaction
            {
                UserId = request.UserId,
                CryptoId = crypto.Id,
                ExchangeId = exchange.Id,
                Action = request.Action,
                CryptoAmount = request.CryptoAmount,
                Money = money,
                Datetime = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            // Proyección directa a DTO
            return new TransactionCreateDto
            {
                
                UserId = transaction.UserId,
                CryptoCode = crypto.Code,
                ExchangeCode = exchange.Code,
                Action = transaction.Action,
                CryptoAmount = transaction.CryptoAmount,
                Money = transaction.Money,
                DateTime = transaction.Datetime,
                CreatedAt = transaction.CreatedAt
            };
        }

        public async Task<IEnumerable<TransactionCreateDto>> GetUserTransactionsAsync(int userId)
        {
            return await _context.Transactions
                .Where(t => t.UserId == userId)
                .Include(t => t.Crypto)
                .Include(t => t.Exchange)
                .OrderByDescending(t => t.Datetime)
                .Select(t => new TransactionCreateDto
                {
                    UserId = t.UserId,
                    CryptoCode = t.Crypto.Code,
                    ExchangeCode = t.Exchange.Code,
                    Action = t.Action,
                    CryptoAmount = t.CryptoAmount,
                    Money = t.Money,
                    DateTime = t.Datetime,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<BestPriceDto>> GetBestPricesAsync(string cryptoCode, string action)
        {
            if (action != "purchase" && action != "sale")
                throw new ArgumentException("Acción no válida. Debe ser 'purchase' o 'sale'");

            var allPrices = await _cryptoYaApiClient.GetAllPrices(cryptoCode);

            return action == "purchase"
                ? allPrices.OrderBy(p => p.Price).Take(3)
                : allPrices.OrderByDescending(p => p.Price).Take(3);
        }

        public async Task<IEnumerable<WalletStatusDto>> GetWalletStatusAsync(int userId)
        {
            var walletStatus = await _context.WalletStatusDto
                .FromSqlRaw("SELECT user_id AS UserId, crypto_code AS CryptoCode, crypto_name AS CryptoName, total_amount AS TotalAmount FROM wallet_status WHERE user_id = {0}", userId)
                .ToListAsync();

            var result = new List<WalletStatusDto>();

            foreach (var item in walletStatus)
            {
                var bestPrice = (await GetBestPricesAsync(item.CryptoCode, "sale")).FirstOrDefault();

                result.Add(new WalletStatusDto
                {
                    CryptoCode = item.CryptoCode,
                    CryptoName = item.CryptoName,
                    TotalAmount = item.TotalAmount,
                    CurrentPrice = bestPrice?.Price,
                    CurrentValue = bestPrice != null ? item.TotalAmount * bestPrice.Price : null
                });
            }

            return result;
        }

        public async Task<decimal> GetCryptoBalance(int userId, string cryptoCode)
        {
            return await _context.Database
                .SqlQuery<decimal>($"SELECT get_crypto_balance({userId}, '{cryptoCode}') AS Value")
                .FirstOrDefaultAsync();
        }
    }
}

