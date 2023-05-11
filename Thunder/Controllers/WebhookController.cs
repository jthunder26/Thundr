using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Thunder.Services;
using Thunder.Models;
using Microsoft.AspNetCore.Identity;
using Thunder.Data;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Composition;
using System.Numerics;
using Microsoft.AspNetCore;
using static Google.Protobuf.WellKnownTypes.Field.Types;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Security.Claims;
using System.Runtime.Intrinsics.X86;

namespace Thunder.Controllers
{
    [Route("webhook")]
    [ApiController]
    public class WebhookController : Controller
    {
        private readonly IUserService _userService;

        public WebhookController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpPost]
        public async Task<IActionResult> Index()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            const string endpointSecret = "whsec_3362d4d218dc5080c7b9fb863908b0b3374f6fa8dbcbc57905b1326c96bc8a8d";
            try
            {
                var stripeEvent = EventUtility.ParseEvent(json);
                var signatureHeader = Request.Headers["Stripe-Signature"];

                stripeEvent = EventUtility.ConstructEvent(json,
                        signatureHeader, endpointSecret);

                if (stripeEvent.Type == Events.ChargeSucceeded)
                {
                    var charge = stripeEvent.Data.Object as Charge;
                    Console.WriteLine("A successful payment for {0} was made. In the amount of {1}", charge.BillingDetails.Name, charge.AmountCaptured);

                    // Get the User ID from the metadata or other sources
                    var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    decimal amountInCurrency = charge.AmountCaptured / 100m;

                    // Call the UpdateUserBalanceByUserIdAsync() function
                    var updateResult = await _userService.UpdateUserBalanceByUserIdAsync(uid, amountInCurrency);

                    // Check if the update was successful
                    if (updateResult.Succeeded)
                    {
                        // Handle successful update, e.g., return a success response
                        return Ok("User balance updated successfully.");
                    }
                    else
                    {
                        // Log the error and return a 400 Bad Request response
                        var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                        Console.Error.WriteLine($"Failed to update user balance. Errors: {errors}");
                        return BadRequest("Failed to update user balance.");
                    }
                }
                else if (stripeEvent.Type == Events.ChargeFailed)
                {
                    var charge = stripeEvent.Data.Object as Charge;
                    Console.WriteLine("A failed payment for {0} was made. In the amount of {1}", charge.BillingDetails.Name, charge.Amount);

                    // Get the User ID from the metadata or other sources
                    var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    // Perform any required actions, e.g., update your application's records or send a notification to the user

                    // Return a success response to acknowledge receipt of the event
                    return Ok("Charge failed event received.");
                }
                else
                {
                    // Handle other event types
                }
            }
            catch (StripeException e)
            {
                // Log the error and return a 400 Bad Request response
                Console.Error.WriteLine($"Error: {e.Message}");
                return BadRequest("Error processing the event.");
            }

            // Return a generic response for unhandled events
            return Ok("Event received.");
        }
    }

}
