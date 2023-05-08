using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;
using Thunder.Data;


namespace Thunder.Services
{
    public interface IStripeService
    {
        Task<string> CreateTopUpSessionAsync(string userEmail, decimal topUpAmount);
    }



    public class StripeService : IStripeService
    {
        private readonly ApplicationDbContext _db;

        public StripeService(ApplicationDbContext db)
        {
            _db = db;
        }
        // create Stripe checkout session and return a session ID.
        public async Task<string> CreateTopUpSessionAsync(string userEmail, decimal topUpAmount)
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                    {
                        "card",
                    },
                LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                Currency = "usd",
                                UnitAmount = (long) (topUpAmount * 100), // Convert to cents
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = "Top-up",
                                },
                            },
                            Quantity = 1,
                        },
                    },
                Mode = "payment",
                SuccessUrl = "https://your_domain.com/top-up/success?session_id={{CHECKOUT_SESSION_ID}}", //ADD OUR DOMAIN HERE
                CancelUrl = "https://your_domain.com/top-up/cancel",
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);
            return session.Id;
        }


        public async Task UpdateUserBalanceAsync(Session session)
        {
            // Extract the customer's email from the session
            var customerEmail = session.CustomerDetails.Email;

            // Find the user associated with the email
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == customerEmail);

            if (user == null)
            {
                // Handle the case when a user is not found
                throw new System.Exception("User not found");
            }

            // Find the UserBalance associated with the user
            var userBalance = await _db.UserBalances.SingleOrDefaultAsync(ub => ub.UserId == user.Id);

            if (userBalance == null)
            {
                // If there's no UserBalance, create a new one
                userBalance = new UserBalance
                {
                    UserId = user.Id,
                    Balance = 0
                };
                _db.UserBalances.Add(userBalance);
            }

            // Calculate the amount of the top-up
            var topUpAmount = (session.AmountTotal ?? 0) / 100m;


            // Update the user's balance
            userBalance.Balance += topUpAmount;

            // Save the changes to the database
            await _db.SaveChangesAsync();
        }


        //In this implementation, we're first extracting the customer's email from the session object,
        //    then finding the user and their associated UserBalance. 
        //    If a UserBalance is not found, a new one is created.
        //    Next, we update the user's balance by adding the top-up amount and save the changes to the database.











    }

}
