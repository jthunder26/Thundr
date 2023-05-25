using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Stripe;
using Thunder.Models;
using Thunder.Services;

namespace Thunder.Controllers
{
    [Route("webhook")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IThunderService _thunderService;
        private readonly ILogger<WebhookController> _logger;
        private readonly string _webhookSecret;
        public WebhookController(IUserService userService, ILogger<WebhookController> logger, SecretClient secretClient, IThunderService thunderService)
        {
            _userService = userService;
            _logger = logger;


            var stripeWebhookSecret = secretClient.GetSecret("StripeEndpointSecret");
            _webhookSecret = stripeWebhookSecret.Value.Value;
            _thunderService = thunderService;   
        }

        [HttpPost]
        [AllowAnonymous]
        [Consumes("application/json")]
        public async Task<IActionResult> Index()
        {
            var eventType = "";
            var custID = "";
            try
            {
                var json = await new StreamReader(Request.Body).ReadToEndAsync();

                var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], _webhookSecret);
                eventType = stripeEvent.Type;
                if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                    return Ok("User PaymentIntent Succeeded.");
                }

                if (stripeEvent.Type == Events.PaymentIntentCanceled)
                {
                    return Ok("User PaymentIntent Canceled.");
                }

                if (stripeEvent.Type == Events.ChargeSucceeded)
                {
                    var charge = stripeEvent.Data.Object as Charge;
                    if (charge != null)
                    {
                        //try
                        //{
                        //    _logger.LogInformation("A successful payment for {BillingName} was made. In the amount of {Amount}",
                        //                      charge.BillingDetails.Name, charge.AmountCaptured);
                        //    var stripeCustomerId = charge.CustomerId;

                        //    var labelIdString = charge.Metadata["LabelId"];
                        //    var serviceClass = charge.Metadata["ServiceClass"];
                        //    int labelId = int.Parse(labelIdString);
                        //    long amountCaptured = charge.AmountCaptured;
                        //    var response = await _thunderService.ValidateChargeAsync(labelId, amountCaptured, stripeCustomerId, serviceClass);

                        //    if (response.Success)
                        //    {
                        //        _thunderService.CreateAIOLabelAsync(labelId);
                        //        // If validation was successful, return a success response
                        //        return Ok(response);
                        //    }
                        //    else
                        //    {
                        //        // Log the error and return a Bad Request response with the error message
                        //        _logger.LogError("Failed to validate charge. Error: {Error}", response.Message);
                        //        return BadRequest($"Failed to validate charge. Error: {response.Message}");
                        //    }
                        //}
                        //catch (Exception ex)
                        //{
                        //    _logger.LogError(ex, "Exception occurred while validating charge.");
                        //    return StatusCode(500, "An unexpected error occurred while validating charge."+ ex);
                        //}
                    }
                    else
                    {
                        _logger.LogError("stripeEvent.Data.Object is not of type Charge.");
                        return BadRequest("stripeEvent.Data.Object is not of type Charge.");
                    }
                }
                else if (stripeEvent.Type == Events.ChargeFailed)
                {
                    var charge = stripeEvent.Data.Object as Charge;
                    _logger.LogInformation("A failed payment for {BillingName} was made. In the amount of {Amount}", charge.BillingDetails.Name, charge.Amount);

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
                _logger.LogError("Error processing the event:{Message}", e);
                return BadRequest("Error processing the event: " + e);
            }
            catch (Exception e)
            {
                // Log the error and return a 500 Internal Server Error response
                _logger.LogError("An unexpected error occurred while processing the event: {Message}", e);
                return StatusCode(500, "An unexpected error occurred while processing the event: " + e + "// Here is the event: " + eventType + " //CustomerID from Charge: " + custID);
            }

            // Return a generic response for unhandled events
            return Ok("Event received.");
        }
    }
}
