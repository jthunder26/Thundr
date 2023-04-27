using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.FinancialConnections;
using Thunder.Models;

namespace Thunder.Controllers
{
    //[Route("api/create-payment-intent")]
    //[ApiController]
    //public class PaymentIntentApiController : Controller
    //{
        [Route("create-intent")]
        [ApiController]
        public class CheckoutApiController : Controller
        {
            public CheckoutApiController()
            {
                StripeConfiguration.ApiKey = "pk_test_51MxFCnDHpayIZlcAytKURkjtSmxLNLAd0V2noxps5R1Of0zyHxD67diq4jeehDxzSW2TbyC7Wpu8gDpGi6ros1vU009J6Nf8zm";
            }

            [HttpPost]
            public ActionResult Post()
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = 1099,
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
    }
}
