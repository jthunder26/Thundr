using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.FinancialConnections;
using Thunder.Models;

namespace Thunder.Controllers
{
    
        [Route("create-intent")]
        [ApiController]
        public class CheckoutApiController : Controller
        {
            public CheckoutApiController()
            {
                StripeConfiguration.ApiKey = "sk_test_51MxFCnDHpayIZlcAaiJXTw7ln9gD8sPbzmNtN9bBIwFmhrOMhGcoLlWHkbrE8EHUvYDmsoU7e8iCY0Jh0SWRFH8N00sbrQOelZ";
            }

            [HttpPost]
            public ActionResult Post([FromBody] CreateIntentRequest request)
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = request.Amount,
                    Currency = "usd",
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true,
                    },
                };
                var service = new PaymentIntentService();
                PaymentIntent intent = service.Create(options);
                return Json(new { client_secret = intent.ClientSecret });
            }


        //Zero-decimal currencies-
        //    All API requests expect amounts to be provided in a currency’s smallest unit.
        //    For example, to charge 10 USD, provide an amount value of 1000 (that is, 1000 cents).
        private long CalculateOrderAmount(RateDTO rate)
        {
            string amount = rate.ourPrice;
            if (decimal.TryParse(amount, out decimal price))
            {
                // Convert the price to the smallest currency unit (e.g., cents)
                long smallestCurrencyUnit = (long)(price * 100);
                return smallestCurrencyUnit;
            }
            else
            {
                return 0;
            }
        }

        public class CreateIntentRequest
        {
            public long Amount { get; set; }
        }
    }
}
