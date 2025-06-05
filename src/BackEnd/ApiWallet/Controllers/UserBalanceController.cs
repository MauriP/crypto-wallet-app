using ApiWallet.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApiWallet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserBalanceController : ControllerBase
    {
        private readonly IBalanceService _balanceService;

        public UserBalanceController(IBalanceService balanceService)
        {
            _balanceService = balanceService;
        }

        // Obtiene el balance del usuario, utilizando el metodo GetUserBalanceAsync del BalanceService
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserBalance(int userId)
        {
            try
            {
                var balance = await _balanceService.GetUserBalanceAsync(userId);
                return Ok(balance);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener el balance: {ex.Message}");
            }
        }
    }
}
