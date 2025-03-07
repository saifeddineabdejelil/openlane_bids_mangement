using BidManagement.Models;

namespace BidManagement.Utilis
{
    public static class BidValidator
    {
        public static bool IsValid(Bid bid, out string validationMessage)
        {
            validationMessage = string.Empty;
            if (bid.CarId <= 0)
            {
                validationMessage = "Empty data";
                return false;
            }

            if (bid.CarId <= 0)
            {
                validationMessage = "No car linked to this bid";
                return false;
            }

            if (bid.Amount <= 0)
            {
                validationMessage = "Amount cannot be negative";
                return false;
            }

            return true;
        }
    }
}

