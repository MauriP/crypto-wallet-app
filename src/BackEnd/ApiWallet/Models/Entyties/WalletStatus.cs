using System;
using System.Collections.Generic;

namespace ApiWallet.Models.Entyties;

public partial class WalletStatus
{
    public int UserId { get; set; }

    public string CryptoCode { get; set; } = null!;

    public string CryptoName { get; set; } = null!;

    public decimal? TotalAmount { get; set; }
}
