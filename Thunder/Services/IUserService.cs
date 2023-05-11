using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;
using Thunder.Data;


namespace Thunder.Services
{
    public interface IUserService
    {
        Task<IdentityResult> UpdateUserBalanceByUserIdAsync(string userId, decimal balance);
    }



    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }


        // create Stripe checkout session and return a session ID.
        public async Task<IdentityResult> UpdateUserBalanceByUserIdAsync(string userId, decimal balance)
        {
            // Find the user by their User ID
            var user = await _userManager.FindByIdAsync(userId);

            // If the user is not found, return an error result
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            // Update the user's balance
            user.Balance += balance;

            // Save the changes to the user
            var result = await _userManager.UpdateAsync(user);
            return result;
        }







    }

}
