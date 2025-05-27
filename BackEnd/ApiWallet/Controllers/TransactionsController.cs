using ApiWallet.Services.Interfaces;
using ApiWallet.Models.DTos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiWallet.Controllers
{
    // TransactionsController.cs
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(ITransactionService transactionService, ILogger<TransactionsController> logger)
        {
            _transactionService = transactionService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] TransactionRequestDto request)
        {
            try
            {
                var transaction = await _transactionService.CreateTransactionAsync(request);
                return CreatedAtAction(nameof(GetUserTransactions), new { userId = transaction.UserId }, transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear transacción");
                return BadRequest(new { message = ex.Message });
            }
        }

        // Historial de transacciones de un usuario
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserTransactions(int userId)
        {
            try
            {
                var transactions = await _transactionService.GetWalletStatusAsync(userId);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener transacciones para usuario {userId}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("best-prices/{cryptoCode}")]
        public async Task<IActionResult> GetBestPrices(string cryptoCode, [FromQuery] string action)
        {
            try
            {
                var prices = await _transactionService.GetBestPricesAsync(cryptoCode, action);
                return Ok(prices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener mejores precios para {cryptoCode}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // Estado de wallet de un usuario
        [HttpGet("wallet-status/{userId}")]
        public async Task<IActionResult> GetWalletStatus(int userId)
        {
            try
            {
                var status = await _transactionService.GetWalletStatusAsync(userId);
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener estado de wallet para usuario {userId}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // Balance de criptomonedas de un usuario
        [HttpGet("balance/{userId}/{cryptoCode}")]
        public async Task<IActionResult> GetCryptoBalance(int userId, string cryptoCode)
        {
            try
            {
                var balance = await _transactionService.GetCryptoBalance(userId, cryptoCode);
                return Ok(new { balance });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener balance para usuario {userId} y cripto {cryptoCode}");
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
