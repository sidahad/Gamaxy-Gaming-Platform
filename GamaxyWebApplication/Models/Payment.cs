using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GamaxyWebApplication.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        public int? UserId { get; set; }
        public int? GamerId { get; set; }
        public int GameId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } // e.g., "Pending", "Completed", "Failed"
        public DateTime PaymentDate { get; set; }
        public string TransactionId { get; set; } // For payment gateway transaction ID

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("GamerId")]
        public Gamer? Gamer { get; set; }

        [ForeignKey("GameId")]
        public Game? Game { get; set; }
    }
}