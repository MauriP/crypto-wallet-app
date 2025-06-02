using System;
using System.Collections.Generic;

namespace ApiWallet.Models.Entyties;

public partial class VTransactionHistory
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string CryptoCode { get; set; } = null!;

    public string CryptoName { get; set; } = null!;

    public string? ExchangeCode { get; set; }

    public string? ExchangeName { get; set; }

    public string Action { get; set; } = null!;

    public decimal CryptoAmount { get; set; }

    public decimal Money { get; set; }

    public DateTime Datetime { get; set; }

    public DateTime? CreatedAt { get; set; }
}
