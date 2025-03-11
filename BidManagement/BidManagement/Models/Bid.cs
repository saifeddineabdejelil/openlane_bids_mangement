using System.ComponentModel.DataAnnotations.Schema;

namespace BidManagement.Models
{
    public class Bid
    {
        public int Id { get; set; }

        [Column(TypeName = "DECIMAL(18,2)")]
        public decimal Amount { get; set; }
        public int CarId { get; set; }
        public string ClientEmail { get; set; }

        public DateTime BidTime { get; set; } = DateTime.UtcNow;


    }
}
