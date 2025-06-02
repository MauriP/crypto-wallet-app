using ApiWallet.Data;
using ApiWallet.Services.Implemetaciones;
using ApiWallet.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiWallet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PriceController : ControllerBase
    {
        private readonly WalletDbContext _context;
        private readonly PriceUpdaterService _priceUpdater;
        private readonly ITransactionService _transactionService;

        public PriceController(WalletDbContext context, PriceUpdaterService priceUpdater, ITransactionService transactionService)
        {
            _context = context;
            _priceUpdater = priceUpdater;
            _transactionService = transactionService;
        }

        // Obtener el precio de compra de una criptomoneda en un exchange específico
        [HttpGet("buy/{cryptoCode}/{exchangeCode}")]
        public async Task<IActionResult> GetBuyPrice(string cryptoCode, string exchangeCode)
        {
            var price = await _transactionService.GetBuyPriceAsync(cryptoCode, exchangeCode);
            return Ok(new { price });
        }

        // Obterner el precio de venta de una criptomoneda en un exchange específico
        [HttpGet("sell/{cryptoCode}/{exchangeCode}")]
        public async Task<IActionResult> GetSellPrice(string cryptoCode, string exchangeCode)
        {
            var price = await _transactionService.GetSellPriceAsync(cryptoCode, exchangeCode);
            return Ok(new { price });
        }

        // POST: api/prices/update
        [HttpPost("update")]
        public async Task<IActionResult> UpdateAllPrices()
        {
            var cryptos = await _context.Cryptocurrencies
                .Select(c => c.Code)
                .ToListAsync();

            foreach (var cryptoCode in cryptos)
            {
                await _priceUpdater.UpdatePricesAsync(cryptoCode);
            }

            return Ok(new { message = "Precios actualizados correctamente" });
        }


        // GET: api/prices
        [HttpGet]
        public async Task<IActionResult> GetLatestPrices()
        {
            var allPrices = await _context.CryptoPrices
                .Include(cp => cp.Crypto)
                .Include(cp => cp.Exchange)
                .ToListAsync(); // Trae todo a memoria para operar agrupamientos complejos

            var latestPrices = allPrices
                .GroupBy(cp => new { cp.CryptoId, cp.ExchangeId })
                .Select(g => g.OrderByDescending(p => p.LastUpdated).First())
                .Select(cp => new
                {
                    Crypto = cp.Crypto.Code,
                    Exchange = cp.Exchange.Code,
                    BuyPrice = cp.BuyPrice,
                    SellPrice = cp.SellPrice,
                    LastUpdated = cp.LastUpdated
                })
                .ToList();

            return Ok(latestPrices);
        }
        [HttpGet("ping")]
        public IActionResult Ping() => Ok("pong");

        // Actuqalizar precios de una criptomoneda específica seugn el exchange y la accion
        [HttpGet("total/{operation}/{cryptoCode}/{exchangeCode}")]
        public async Task<IActionResult> GetTotalInArs(
            string operation,
            string cryptoCode,
            string exchangeCode,
            [FromQuery] decimal amount)
        {
            decimal? price = null;
            if (operation == "purchase")
                price = await _transactionService.GetBuyPriceAsync(cryptoCode.ToUpper(), exchangeCode.ToLower());
            else if (operation == "sale")
                price = await _transactionService.GetSellPriceAsync(cryptoCode.ToUpper(), exchangeCode.ToLower());

            if (price == null)
                return BadRequest(new { message = "No se pudo obtener el precio" });

            var total = price.Value * amount;
            return Ok(new { total });
        }
    }
}

