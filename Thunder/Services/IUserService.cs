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
using Microsoft.Azure.Documents;


namespace Thunder.Services
{
    public interface IUserService
    {
        Task<ApplicationUser> GetCurrentUserAsync();
        string GetCurrentUserId();
        Task<UserDeets> GetUserDeetsAsync(string uid);
        Task<FindUserResult> FindByStripeCustomerIdAsync(string stripeCustomerId);
        //Task<ChargeValidationResult> UpdateUserBalanceByCustomerIdAsync(string stripeCustomerId, long amountCaptured);
        //Task<IdentityResult> ChargeCustomer(string stripeCustomerId, int charge);
        Task<decimal> GetUserBalance(string uid);
        Task<decimal> GetUserBalanceByEmail(string email);
        Task<string> GetStripeCustomerId(string userId);
        Task<bool> UpdateUserBalance(string userId, int balanceToAdd);
        Task<bool> AdminUpdateUserBalance(UserToUpdate user);
        Task<ChargeValidationResult> ChargeUserBalance(string userId, int chargeAmount);
    }



    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
       
        
        public UserService(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
           
            _httpContextAccessor = httpContextAccessor;
            
        }


        public async Task<UserDeets> GetUserDeetsAsync (string uid)
        {
            var user = await _userManager.FindByIdAsync(uid);
            UserDeets ud = new UserDeets();
            ud.FullName = user.FullName;
            ud.Email = user.Email;
            return ud;  
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
       
        public async Task<bool> AdminUpdateUserBalance(UserToUpdate userToUpdate)
        {
            // Find user by user ID
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == userToUpdate.email);
            if (user == null)
            {
                return false;  // User not found
            }
            
            // Add balance to user
            user.UserBalance = int.Parse(userToUpdate.balance)*100;

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
                var unfinishedLabelDetails = _db.LabelDetail.FirstOrDefault(x => x.Uid == userId && x.Status == 1);
                unfinishedLabelDetails.Message = "Not Enough Balance. User Balance: " + user.UserBalance / 100 + ".Label Cost: " + chargeAmount / 100;
                unfinishedLabelDetails.Error = 1;
                _db.SaveChanges();
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
               // _logger.LogError(ex, "User not found");

                // Return an error response indicating the failure
                return 0; // Or any other appropriate response
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error occurred while retrieving user balance");

                // Return a generic error response
                return 0; // Or any other appropriate response
            }
        }  
        public async Task<decimal> GetUserBalanceByEmail(string email)
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    throw new NotFoundException("User not found");
                }

                return user.UserBalance;
            }
            catch (NotFoundException ex)
            {
               // _logger.LogError(ex, "User not found");

                // Return an error response indicating the failure
                return 0; // Or any other appropriate response
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error occurred while retrieving user balance");

                // Return a generic error response
                return 0; // Or any other appropriate response
            }
        }









    }

}

