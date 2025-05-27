using System;
using System.Collections.Generic;

namespace ApiWallet.Models.Entyties;

public partial class VWalletSummary
{
    public int UserId { get; set; }

    public string CryptoCode { get; set; } = null!;

    public string CryptoName { get; set; } = null!;

    public decimal? TotalAmount { get; set; }

    public decimal? CurrentPrice { get; set; }

    public decimal? CurrentValue { get; set; }
}
