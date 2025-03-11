using System.ComponentModel.DataAnnotations.Schema;

namespace BidManagement.Models
{
    public class Car
    {
        public int Id { get; set; }
        public string Model { get; set; }

        [Column(TypeName = "DECIMAL(18,2)")]
        public decimal Price { get; set; }
        public string ClientEmail { get; set; }
        public ICollection<Decision> Decisions { get; set; }


    }
}
