using System.ComponentModel.DataAnnotations;
namespace ApiWallet.Models.Entyties
{
    public class UserPesosBalance
    {
        
        public int UserId { get; set; }
        public decimal PesosBalance { get; set; }
        public virtual User User { get; set; } = null!;
    }
}
