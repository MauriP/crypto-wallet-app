namespace ApiWallet.Models.DTos
{
    public class CryptoBalanceDetailDto
    {
        public string CryptoCode { get; set; }
        public decimal? CryptoAmount { get; set; }
        public decimal? CurrentPrice { get; set; }
        public decimal? ValueInARS { get; set; }
    }
}
