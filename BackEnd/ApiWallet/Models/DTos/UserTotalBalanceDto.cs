namespace ApiWallet.Models.DTos
{
    public class UserTotalBalanceDto
    {
        public int UserId { get; set; }
        public decimal? TotalBalanceInARS { get; set; }
        public List<CryptoBalanceDetailDto> CryptoBalances { get; set; }
    }
}
