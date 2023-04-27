namespace Thunder.Services
{
    public interface IStripeService
    {
        Task<string> CreateTopUpSessionAsync(string userEmail, decimal topUpAmount);
    }

}
