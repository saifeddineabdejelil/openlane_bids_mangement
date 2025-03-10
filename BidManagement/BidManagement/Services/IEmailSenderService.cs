namespace BidManagement.Services
{
    public interface IEmailSenderService
    {
        Task SendBidWinningEmailAsync(string clientEmail, string subject, string body);

    }
}
