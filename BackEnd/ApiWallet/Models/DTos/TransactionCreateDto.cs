using System.ComponentModel.DataAnnotations;

public class TransactionCreateDto
{
    [Required]
    public int UserId { get; set; }
    [Required]
    public string CryptoCode { get; set; }
    public string ExchangeCode { get; set; }
    [Required]
    [Range(0.00000001, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public decimal CryptoAmount { get; set; }
    public decimal Money { get; set; }
    [Required]
    public string Action { get; set; } // "purchase" o "sale"
    public DateTime DateTime { get; set; }
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow; // Por defecto, la fecha de creación es ahora

}
