﻿using ApiWallet.Services.Interfaces;
using ApiWallet.Models.DTos;
using Microsoft.AspNetCore.Mvc;

namespace ApiWallet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserPesosBalanceController : ControllerBase
    {
        private readonly IUserPesosBalanceService _service;

        public UserPesosBalanceController(IUserPesosBalanceService service)
        {
            _service = service;
        }

        // Obtener el saldo de un usuario, usando el metodo GetBalanceAsync del servicio UserPesosBalanceService
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetBalance(int userId)
        {
            var balance = await _service.GetBalanceAsync(userId);
            return Ok(balance);
        }

        // Agregar saldo a un usuario, usando el metodo AddBalanceAsync del servicio
        [HttpPost("add")]
        public async Task<IActionResult> AddBalance([FromBody] UserPesosBalanceDto dto)
        {
            if (dto == null || dto.PesosBalance <= 0)
                return BadRequest("Monto inválido");

            await _service.AddBalanceAsync(dto.UserId, dto.PesosBalance);
            return Ok(new { message = "Saldo cargado correctamente" });
        }
    }
}
