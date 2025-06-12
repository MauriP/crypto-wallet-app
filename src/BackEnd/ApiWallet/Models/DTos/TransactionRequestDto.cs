using System.ComponentModel.DataAnnotations;
public class TransactionRequestDto
{
    public int UserId { get; set; }
    [Required]
    public string CryptoCode { get; set; }
    public string ExchangeCode { get; set; }
    [Required]
    public string Action { get; set; } // "purchase" o "sale"
    [Required]
    public decimal CryptoAmount { get; set; }
    public DateTime DateTime { get; set; } 


}