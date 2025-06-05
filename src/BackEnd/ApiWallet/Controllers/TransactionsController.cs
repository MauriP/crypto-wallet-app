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

        // Crear una nueva transacción llamo al metodo CreateTransactionAsync del servicio
        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] TransactionRequestDto request)
        {
            try
            {
                var transaction = await _transactionService.CreateTransactionAsync(request);
                return Ok(new
                {
                    message = "Transacción creada exitosamente",
                    transaction
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear transacción");
                return BadRequest(new { message = ex.Message });
            }
        }

        // Historial de transacciones de un usuario, llamo al metodo GetUserTransactionsAsync del servicio
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserTransactions(int userId)
        {
            try
            {
                var transactions = await _transactionService.GetUserTransactionsAsync(userId);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener transacciones para usuario {userId}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // Datos de una transaccion específica del usuario llamo al metodo GetTransactionByIdAsync del servicio
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransactionById(int id)
        {
            try
            {
                var transaction = await _transactionService.GetTransactionByIdAsync(id);
                if (transaction == null)
                    return NotFound(new { message = "Transacción no encontrada" });

                return Ok(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener transacción con id {id}");
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


        // Balance de una criptomoneda en espefico de un usuario llamo al metodo GetCryptoBalance del servicio
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
