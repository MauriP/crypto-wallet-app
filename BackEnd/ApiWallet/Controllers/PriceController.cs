using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiWallet.Services.Implemetaciones;
using ApiWallet.Data;

namespace ApiWallet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PriceController : ControllerBase
    {
        private readonly WalletDbContext _context;
        private readonly PriceUpdaterService _priceUpdater;

        public PriceController(WalletDbContext context, PriceUpdaterService priceUpdater)
        {
            _context = context;
            _priceUpdater = priceUpdater;
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


    }
}

