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

namespace Thunder.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StripeController : ControllerBase
    {
        private readonly ILogger<StripeController> _logger;
        private readonly IOptions<StripeOptions> _stripeOptions;
        private readonly StripeService _stripeService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        public StripeController(ILogger<StripeController> logger, IOptions<StripeOptions> stripeOptions, 
            StripeService stripeService, UserManager<ApplicationUser> userManager,
            ApplicationDbContext db)
        {
            _logger = logger;
            _stripeOptions = stripeOptions;
            _stripeService = stripeService;
            _userManager = userManager;
            _db = db;
        }
        //Create a webhook to listen for the checkout.session.completed event.
        //When this event is received, update the user's balance by adding the top-up amount.
        //This will listen for events from Stripe and forward them to your local webhook endpoint.
        //Keep in mind that when you deploy your application, you will need to update the webhook URL in your Stripe Dashboard to point to your live application's URL.
        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], _stripeOptions.Value.WebhookSecret);

                if (stripeEvent.Type == Events.CheckoutSessionCompleted)
                {
                    var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                    if (session != null)
                    {
                        // Add the logic to update the user balance based on the amount in the session.
                        await _stripeService.UpdateUserBalanceAsync(session);
                    }
                }

                return Ok();
            }
            catch (StripeException e)
            {
                _logger.LogError($"Something went wrong with the Stripe webhook: {e.Message}");
                return BadRequest();
            }
            catch (System.Exception e)
            {
                _logger.LogError($"Something went wrong: {e.Message}");
                return StatusCode(500);
            }
        }



        [Authorize]
        [HttpGet]
        public async Task<IActionResult> TopUp(decimal amount)
        {
            var userEmail = User.Identity.Name; // Get the current user's email
            var sessionId = await _stripeService.CreateTopUpSessionAsync(userEmail, amount);
            return Ok(new { sessionId });
        }


        [Authorize]
        [Route("api/[controller]")]
        [HttpGet("GetUserBalance")]
        public async Task<IActionResult> GetUserBalance()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var userBalance = _db.UserBalances.FirstOrDefault(ub => ub.UserId == user.Id);

            if (userBalance == null)
            {
                return NotFound("User balance not found.");
            }

            return Ok(new { balance = userBalance.Balance });
        }
    }
}
