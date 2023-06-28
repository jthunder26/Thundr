using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Stripe;
using Thunder.Models;
using Thunder.Services;

[Route("webhook")]
[ApiController]
public class WebhookController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IThunderService _thunderService;
   
    private readonly string _webhookSecret;
    private readonly IBackgroundQueueService _backgroundQueueService;

    public WebhookController(IUserService userService, ILogger<WebhookController> logger, SecretClient secretClient, IThunderService thunderService, IBackgroundQueueService backgroundQueueService)
    {
        _userService = userService;
       
        _backgroundQueueService = backgroundQueueService;
        var stripeWebhookSecret = secretClient.GetSecret("StripeEndpointSecret");
        _webhookSecret = stripeWebhookSecret.Value.Value;
        _thunderService = thunderService;
    }

    [HttpPost]
    [AllowAnonymous]
    [Consumes("application/json")]
    public async Task<IActionResult> Index()
    {
        var json = await new StreamReader(Request.Body).ReadToEndAsync();

        try
        {

            var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], _webhookSecret);


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
                    //_logger.LogInformation("A successful payment for {BillingName} was made. In the amount of {Amount}", charge.BillingDetails.Name, charge.AmountCaptured);

                    var stripeCustomerId = charge.CustomerId;
                    var labelIdString = charge.Metadata["LabelId"];
                    var serviceClass = charge.Metadata["ServiceClass"];
                    var labelCostFromStripe = int.Parse(charge.Metadata["LabelCost"]);
                    int labelId = int.Parse(labelIdString);
                    int amountCaptured = (int)charge.AmountCaptured;

                    var result = await _userService.FindByStripeCustomerIdAsync(stripeCustomerId);
                    if (result.Success)
                    {
                        var updated = await _userService.UpdateUserBalance(result.uid, amountCaptured);

                        if (updated)
                        {
                            var labelCost = _thunderService.GetLabelCost(labelId, result.uid);
                            if (labelCost == labelCostFromStripe)
                            {

                                var result2 = await _userService.ChargeUserBalance(result.uid, labelCost);
                                if (result2.Success)
                                {
                                    var response = _thunderService.UpdateUnfinishedOrder(labelId, serviceClass, result.uid);
                                    _backgroundQueueService.EnqueueCreateLabel(labelIdString);
                                    return Ok(result2.Message);
                                }

                            }
                            else
                            {
                                //_logger.LogError("Failed to update User balance. Error: {Error}", result.uid);
                                return BadRequest($"Label Cost from Stripe: "+labelCostFromStripe.ToString()+ " does not equal Label Cost from Order: "+labelCost);
                            }

                        }
                        else
                        {
                            //_logger.LogError("Failed to update User balance. Error: {Error}", result.uid);
                            return BadRequest($"Failed to update user balance. Error: {result.uid}");
                        }
                    }
                    else
                    {
                        //_logger.LogError("Failed to find User with this customer ID. Error: {Error}", result.Message);
                        return BadRequest($"Failed to find User with this customer ID. Error: {result.Message}");
                    }
                }
                else
                {
                    //_logger.LogError("stripeEvent.Data.Object is not of type Charge.");
                    return BadRequest("stripeEvent.Data.Object is not of type Charge.");
                }
            }
            else
            {
                //_logger.LogInformation("Unhandled event type: {EventType}", stripeEvent.Type);
            }
        }
        catch (StripeException e)
        {
            //_logger.LogError(e, "A Stripe error occurred while processing the Stripe webhook.");
            return BadRequest();
        }
        return Ok();
    }
}
