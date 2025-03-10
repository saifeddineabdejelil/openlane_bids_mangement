namespace BidManagement.Models
{
    public class Decision
    {
        public int Id { get; set; }
        public int BidId { get; set; }
        public int CarId { get; set; }

        public bool IsWinning { get; set; }
        public Bid Bid { get; set; }
        public Car Car { get; set; }

    }
}
