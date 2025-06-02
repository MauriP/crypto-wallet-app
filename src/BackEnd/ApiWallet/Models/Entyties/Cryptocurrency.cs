using System;
using System.Collections.Generic;

namespace ApiWallet.Models.Entyties;

public partial class Cryptocurrency
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<CryptoPrice> CryptoPrices { get; set; } = new List<CryptoPrice>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
