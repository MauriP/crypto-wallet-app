using System;
using System.Collections.Generic;

namespace ApiWallet.Models.Entyties;

public partial class Transaction
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int CryptoId { get; set; }

    public int? ExchangeId { get; set; }

    public string Action { get; set; } = null!;

    public decimal CryptoAmount { get; set; }

    public decimal Money { get; set; }

    public DateTime Datetime { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Cryptocurrency Crypto { get; set; } = null!;

    public virtual Exchange? Exchange { get; set; }

    public virtual User User { get; set; } = null!;
}
