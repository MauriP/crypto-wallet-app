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
        private readonly IUserPesosBalanceService _userPesosBalanceService;

        public TransactionService(WalletDbContext context, ICryptoYaApiCliente cryptoYaApiClient, ILogger<TransactionService> logger, IUserPesosBalanceService userPesosBalanceService)
        {
            _context = context;
            _cryptoYaApiClient = cryptoYaApiClient;
            _logger = logger;
            _userPesosBalanceService = userPesosBalanceService;
        }

        public async Task<TransactionCreateDto> CreateTransactionAsync(TransactionRequestDto request)
        {
            if (request.CryptoAmount <= 0)
                throw new ArgumentException("El monto de la criptomoneda debe ser mayor que cero");

            var crypto = await _context.Cryptocurrencies.FirstOrDefaultAsync(c => c.Code == request.CryptoCode)
                ?? throw new KeyNotFoundException("Criptomoneda no encontrada");

            var exchange = await _context.Exchanges.FirstOrDefaultAsync(e => e.Code == request.ExchangeCode)
                ?? throw new KeyNotFoundException("Exchange no encontrado");

            decimal price;
            decimal totalMoney; // Renamed variable to avoid CS0136 conflict
            if (request.Action == "purchase")
            {
                price = await _cryptoYaApiClient.GetBuyPriceAsync(request.CryptoCode, request.ExchangeCode);
                totalMoney = request.CryptoAmount * price;

                var userBalance = await _userPesosBalanceService.GetBalanceAsync(request.UserId);
                if (userBalance.PesosBalance < totalMoney)
                    throw new InvalidOperationException("Saldo insuficiente para realizar la compra");
            }
            else if (request.Action == "sale")
            {
                var balance = await GetCryptoBalance(request.UserId, request.CryptoCode);
                _logger.LogInformation("Balance obtenido para el usuario {UserId}: {Balance}", request.UserId, balance);

                if (balance < request.CryptoAmount - 0.00000001m)
                    throw new InvalidOperationException("Saldo insuficiente para realizar la venta");

                price = await _cryptoYaApiClient.GetSellPriceAsync(request.CryptoCode, request.ExchangeCode);
                totalMoney = request.CryptoAmount * price; 
            }
            else
            {
                throw new ArgumentException("Acción no válida. Debe ser 'purchase' o 'sale'");
            }

            var transaction = new Transaction
            {
                UserId = request.UserId,
                CryptoId = crypto.Id,
                ExchangeId = exchange.Id,
                Action = request.Action,
                CryptoAmount = request.CryptoAmount,
                Money = totalMoney, // Use renamed variable
                Datetime = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

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
        public async Task<TransactionCreateDto?> GetTransactionByIdAsync(int id)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Crypto)
                .Include(t => t.Exchange)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
                return null;

            return new TransactionCreateDto
            {
                UserId = transaction.UserId,
                CryptoCode = transaction.Crypto.Code,
                ExchangeCode = transaction.Exchange?.Code,
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
            try
            {
                // 1. Verifica que exista la criptomoneda
                var cryptoExists = await _context.Cryptocurrencies
                    .AnyAsync(c => c.Code == cryptoCode);
                if (!cryptoExists) return 0;

                // 2. Prepara el SQL y usa parámetros para evitar inyección
                var sql = @"
            SELECT 
                COALESCE(SUM(
                    CASE 
                        WHEN t.action = 'purchase' THEN t.crypto_amount 
                        WHEN t.action = 'sale' THEN -t.crypto_amount 
                    END
                ), 0) AS Balance
            FROM transactions t
            JOIN cryptocurrencies c ON t.crypto_id = c.id
            WHERE t.user_id = @userId AND LOWER(c.code) = LOWER(@cryptoCode)";

                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = sql;
                    command.CommandType = System.Data.CommandType.Text;

                    // Parámetros seguros
                    var userIdParam = command.CreateParameter();
                    userIdParam.ParameterName = "@userId";
                    userIdParam.Value = userId;
                    command.Parameters.Add(userIdParam);

                    var cryptoCodeParam = command.CreateParameter();
                    cryptoCodeParam.ParameterName = "@cryptoCode";
                    cryptoCodeParam.Value = cryptoCode;
                    command.Parameters.Add(cryptoCodeParam);

                    await _context.Database.OpenConnectionAsync();
                    var result = await command.ExecuteScalarAsync();
                    var balance = result != DBNull.Value ? Convert.ToDecimal(result) : 0;

                    _logger.LogInformation($"Balance real calculado para {userId}-{cryptoCode}: {balance}");
                    return balance;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error crítico al obtener balance para {userId}-{cryptoCode}");
                throw;
            }
        }
    }
}