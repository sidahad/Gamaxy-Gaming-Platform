using System.ComponentModel.DataAnnotations;

namespace GamaxyWebApplication.Models
{
    public class PaymentViewModel
    {
        public int GameId { get; set; }
        public string GameTitle { get; set; }
        public decimal Amount { get; set; }
        public int UserId { get; set; }
        public string Role { get; set; }

        [Required(ErrorMessage = "Card number is required")]
        [CreditCard]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Expiry date is required")]
        public string ExpiryDate { get; set; }

        [Required(ErrorMessage = "CVV is required")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV must be 3 or 4 digits")]
        public string CVV { get; set; }
    }
}