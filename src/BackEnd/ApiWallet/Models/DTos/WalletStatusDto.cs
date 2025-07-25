﻿using Microsoft.EntityFrameworkCore;

namespace ApiWallet.Models.DTos
{
    [Keyless]
    public class WalletStatusDto
    {
        public string CryptoCode { get; set; }
        public string CryptoName { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? CurrentPrice { get; set; }
        public decimal? CurrentValue { get; set; }
    }
}
