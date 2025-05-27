using System.ComponentModel.DataAnnotations;
namespace ApiWallet.Models.DTos
{
    public class UserLoginDto
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = null!;

        [Required]
        [MinLength(8)]
        public string PasswordHash { get; set; } = null!;
    }
}
