using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing;
using Newtonsoft.Json;
using SendGrid.Helpers.Errors.Model;
using Stripe.Checkout;
using System.Reflection.Metadata;
using System;
using Thunder.Data;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Security.Policy;
using Thunder.Models;


namespace Thunder.Services
{
    public interface IUserService
    {
        Task<ApplicationUser> GetCurrentUserAsync();
        string GetCurrentUserId();
        Task<FindUserResult> FindByStripeCustomerIdAsync(string stripeCustomerId);
        //Task<ChargeValidationResult> UpdateUserBalanceByCustomerIdAsync(string stripeCustomerId, long amountCaptured);
        //Task<IdentityResult> ChargeCustomer(string stripeCustomerId, int charge);
        Task<decimal> GetUserBalance(string uid);
        Task<string> GetStripeCustomerId(string userId);
        Task<bool> UpdateUserBalance(string userId, int balanceToAdd);
        Task<ChargeValidationResult> ChargeUserBalance(string userId, int chargeAmount);
    }



    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserService> _logger;
        
        public UserService(UserManager<ApplicationUser> userManager, ILogger<UserService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            
        }




        public async Task<bool> UpdateUserBalance(string userId, int balanceToAdd)
        {
            // Find user by user ID
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;  // User not found
            }

            // Add balance to user
            user.UserBalance += balanceToAdd;

            // Update user in the database
            var result = await _userManager.UpdateAsync(user);

            // Return whether the operation was successful
            return result.Succeeded;
        }

        public async Task<ChargeValidationResult> ChargeUserBalance(string userId, int chargeAmount)
        {
            // Find user by user ID
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new ChargeValidationResult
                {
                    Success = false,
                    Message = "User not found"
                }; 
            }

            // Check if the user has enough balance to be charged
            if (user.UserBalance < chargeAmount)
            {

                return new ChargeValidationResult
                {
                    Success = false,
                    Message = "Not Enough Balance. User Balance: " + user.UserBalance / 100 + ". Label Cost: " + chargeAmount / 100
                };   // Insufficient balance
            }

            // Deduct the charge amount from the user's balance
            user.UserBalance -= chargeAmount;

            // Update user in the database
            var result = await _userManager.UpdateAsync(user);

            // Return whether the operation was successful
            return new ChargeValidationResult
            {
                Success = true,
                Message = "Charged Successfully"
            };
        }







        public string GetCurrentUserId()
        {
            string userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userId;
        }
        [Authorize] 
       public async Task<ApplicationUser> GetCurrentUserAsync()
        {
            string userId = GetCurrentUserId();
             var user = await _userManager.FindByIdAsync(userId);
            return user;
        }
        public async Task<string> GetStripeCustomerId(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            return user.StripeCustomerId;
        }

        public async Task<FindUserResult> FindByStripeCustomerIdAsync(string stripeCustomerId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.StripeCustomerId == stripeCustomerId);

            if (user == null)
            {
                return new FindUserResult
                {
                    Success = false,
                    Message = "User not found with StripeCustomerId:" + stripeCustomerId
                };
            }

            return new FindUserResult
            {
                Success = true,
                uid = user.Id
            };
        }

        //public async Task<ChargeValidationResult> UpdateUserBalanceByCustomerIdAsync(string stripeCustomerId, long amountCaptured)
        //{
        //    var findResult = await FindByStripeCustomerIdAsync(stripeCustomerId);

        //    if (!findResult.Success)
        //    {
        //        return new ChargeValidationResult { Success = false, Message = findResult.Message };
        //    }

        //    if (amountCaptured > int.MaxValue || amountCaptured < int.MinValue)
        //    {
        //        return new ChargeValidationResult { Success = false, Message = "The amount captured is too large to be processed." };
        //    }

        //    findResult.User.UserBalance += (int)amountCaptured;
        //    var updateResult = await _userManager.UpdateAsync(findResult.User);

        //    if (!updateResult.Succeeded)
        //    {
        //        return new ChargeValidationResult { Success = false, Message = string.Join(", ", updateResult.Errors.Select(x => x.Description)) };
        //    }

        //    return new ChargeValidationResult { Success = true };
        //}


        //public async Task<IdentityResult> ChargeCustomer(string uid, int charge)
        //{
        //    try
        //    {
              
        //        var user = findUserResult.User;
        //        if (user == null)
        //        {
        //            return IdentityResult.Failed(new IdentityError { Description = "User not found." });
        //        }

        //        user.UserBalance -= charge;
        //        var result = await _userManager.UpdateAsync(user);
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"An error occurred while charging the customer with id: {stripeCustomerId}");
        //        return IdentityResult.Failed(new IdentityError { Description = $"An error occurred while charging: {ex.Message}" });
        //    }
        //}


        public async Task<decimal> GetUserBalance(string uid)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(uid);
                if (user == null)
                {
                    throw new NotFoundException("User not found");
                }

                return user.UserBalance;
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex, "User not found");

                // Return an error response indicating the failure
                return 0; // Or any other appropriate response
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user balance");

                // Return a generic error response
                return 0; // Or any other appropriate response
            }
        }









    }

}
