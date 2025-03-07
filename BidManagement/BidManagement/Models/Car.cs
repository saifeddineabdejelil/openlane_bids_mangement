namespace BidManagement.Models
{
    public class Car
    {
        public int Id { get; set; }
        public string Model { get; set; }
        public decimal Price { get; set; }
        public string ClientEmail { get; set; }
        public decimal MinimumBid { get; set; }
        public ICollection<Decision> Decisions { get; set; }


    }
}
