namespace ApiWallet.Models.DTos
{
    public class BestPriceDto
    {
        public string ExchangeCode { get; set; } = null!;
        public decimal Price { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
