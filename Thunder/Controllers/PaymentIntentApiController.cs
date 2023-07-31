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



            _thunderService.UpdateOrder(request.amount, request.description, request.serviceClass, request.charged, request.rateDto.upsPriceOG, request.rateDto.percentSaved);
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
            public long amount { get; set; } //ourPrice 589	long -->$5.89
            public int description { get; set; } //labelId 70	int
            public string serviceClass { get; set; } //class "7deff37b-5900-430c-9335-dabe871bc271"	string
            public long charged { get; set; }   //totalCharge 50	long -->.50 Cents
            public RateDTO rateDto { get; set; } 
         //// ID			    3	int
         //   deliveryDate		"07/17"	string
         //   deliveryDayOfWeek	"Monday"	string
         //   deliveryTime		"11:00 PM"	string
         //   estimatedDelivery	"Estimated Delivery Monday 07/17 by 11:00 PM if shipped today"	string
         //   exactCost		    0	int
         //   isBest			false	bool
         //   isCheapest		true	bool
         //   isFastest		    false	bool
         //   isSelected		true	bool
         //   ourPrice		    "5.89"	string
         //   ourPriceString    "$5.89"	string
         //   percentSaved		"53.86"	string
         //   percentSavedString"Save 53.86%"	string
         //   service			"USPS Priority"	string
         //   serviceClass		"7deff37b-5900-430c-9335-dabe871bc271"	string
         //   ups			    false	bool
         //   upsPrice		    "$12.76 retail"	string
         //   upsPriceOG		13	int
         //   usps			    true	bool

        }
    }
}
