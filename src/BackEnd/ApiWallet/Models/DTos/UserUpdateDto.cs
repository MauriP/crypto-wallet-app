using System.ComponentModel.DataAnnotations;

namespace ApiWallet.Models.DTos
{
    public class UserUpdateDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
        public string? Username { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "El correo no puede exceder 100 caracteres")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
        public string? Email { get; set; }
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        public string? NewPasswordHash { get; set; }
        [Compare("NewPasswordHash", ErrorMessage = "Las contraseñas no coinciden")]
        public string? ConfirmPassword { get; set; }

    }
}
