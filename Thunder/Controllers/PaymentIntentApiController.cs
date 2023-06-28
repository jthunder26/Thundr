using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Threading.Tasks;
using Thunder.Models;
using Thunder.Services;

namespace Thunder.Controllers
{
    [Route("create-intent")]
    [ApiController]
    public class CheckoutApiController : Controller
    {
        private readonly IUserService _userService;
        private readonly IStripeClient _stripeClient;
        private readonly IThunderService _thunderService;
        public CheckoutApiController(IStripeClient stripeClient, IUserService userService, IThunderService thunderService)
        {
            _userService = userService;
            _stripeClient = stripeClient;
            _thunderService = thunderService;   
        }

        [HttpPost]
        public async Task<ActionResult> PostAsync([FromBody] CreateIntentRequest request)
        {
            var user = await _userService.GetCurrentUserAsync();



            _thunderService.UpdateOrder(request.amount, request.description, request.serviceClass, request.charged);
            var options = new PaymentIntentCreateOptions
            {
                Amount = request.charged,
                Currency = "usd",
                Customer = user.StripeCustomerId,
                ReceiptEmail = user.Email,
                Metadata = new Dictionary<string, string>
                      {
                        { "LabelId", request.description.ToString() },
                        { "ServiceClass", request.serviceClass },
                        { "LabelCost", request.amount.ToString() }
                      },
                
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                }
            };

            var service = new PaymentIntentService(_stripeClient);
            PaymentIntent intent = await service.CreateAsync(options);
            return Json(new { client_secret = intent.ClientSecret });
        }

        public class CreateIntentRequest
        {
            public long amount { get; set; }
            public int description { get; set; }
            public string serviceClass { get; set; }
            public long charged { get; set; }   
          
        }
    }
}
