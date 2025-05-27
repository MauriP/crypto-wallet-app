using System;
using System.Collections.Generic;

namespace ApiWallet.Models.Entyties;

public partial class CryptoPrice
{
    public int Id { get; set; }

    public int CryptoId { get; set; }

    public int ExchangeId { get; set; }

    public decimal? BuyPrice { get; set; }

    public decimal? SellPrice { get; set; }

    public DateTime? LastUpdated { get; set; }

    public virtual Cryptocurrency Crypto { get; set; } = null!;

    public virtual Exchange Exchange { get; set; } = null!;
}
