using System.ComponentModel.DataAnnotations;

namespace ApiWallet.Models.DTos
{
    public class UserRegisterDto
    {
        [Required]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
        public string Username { get; set; } = null!;
        [Required] 
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "El formato es incorrecto")]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(8)]
        public string PasswordHash { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
