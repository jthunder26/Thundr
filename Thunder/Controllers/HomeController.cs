using EllipticCurve.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using NuGet.Protocol;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Policy;
using Thunder.Data;
using Thunder.Models;
using Thunder.Services;

namespace Thunder.Controllers
{
    /// <summary>
    /// 
    /// NEW 
    /// 
    /// Add ability to update TotalCharge for customer in Bulk Shipping
    /// 
    /// 
    /// NOTES: 
    /// 0. shipster USPS

    ///
    /// 6. Update rate dropdown ui to be more legible. 
    /// 
    /// 11. Add a support page, user can send in a email. 
    /// 12. Add Terms and Conditions accept checkbox when user creates account
    /// 
    /// 
    /// 
    /// 
    /// --------------------------------------------------------------------------------------------------------------------------------------------------------------------
    ///     /// 1. When applying discount, first workaround is to apply an additional 15% to the ourPrice. -- DONE
    ///     /// 4. Check Auto Scroll function on Rates and Ship page -- DONE
    ///     /// 10. Remove link to Monitor -- DONE.
    ///     /// 5. Have the Cheapest rate show first instead of Fastest -- DONE
    ///     /// 3. When applying discount to USPS, we use UPSOGPrice, add an initial discount to UPSogPrice before assigning to USPSOgPrice to 
    ///     be more inconspicuous  -- DONE
    ///     7. Admin accounts and pages.  --DONE
    ///              A. Add a balance maintenance page. Admin is able to update users balances. 
    ///              B. Admins to be able to manually reset passwords
    ///              C. Admins to be able to view any errors pertaining to a users orders.
    ///              D. Admins able to generate promo codes. -- Talk about pricing later. 
    ///     /// 9. On payment confirmation page, include link to Orders page, have completed orders tab be active always.  -- DONE
    ///          8. Add duplicate order function -- DONE
    ///          
    /// NEW 1: Check why upsOgPrice and OgPriceString are difference, one is rounded: 
    ///    //   upsPrice	"$12.76 retail"	string
    ///     //   upsPriceOG		13	int --- DONE
    ///  2. Bulk upload
    ///     A. No validation
    ///     B. First, validate values in columns. After validation show confirmation screen with errors or with the order details, showing 
    ///     numbered list of labels to be created. Then below show order summary allow user to select different service classes, dynamically 
    ///     update the order form.  --- IN PROGRESS
    /// </summary>

    /// <summary>

    /// 
    /// #dec880
    /// #ff347d
    /// #827ded
    /// 
    /// Orders page, unfinished orders colored baclground built bad in smaller window size
    /// 
    /// Return Address is not saving on registry. -- done
    /// Balance Functions -- done
    /// Create Email Templates for Account Creation, Payment Confirmation, Password Reset. 
    /// Create Blob Storage to store labels -- done
    /// Transfer Domain to cloudflare -- done
    /// Add the auto scroll -- Done 
    /// Check error from createLabel() to Aio, if Success is false and Error is not over balance issues/format
    /// --> Switch API's
    /// 
    /// 
    /// Items to be added in the future:
    /// 
    /// Add auto complete for Ship From as well. 
    /// Add Content to homepage
    /// About/Contact Us pages/ FAQS/ Support to submit bugs
    /// USPS Integration/Price added to rates
    /// Add Promo Code Logic
    /// View Tracking Info/ Tracking Map
    /// Duplicate previous order
    /// Spreadsheet functionality

    /// </summary>


    public class HomeController : Controller
    {
       
        private readonly IThunderService _thunderService;
        private readonly IUpsRateService _upsRateService;
        private readonly IMailService _mailService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserService _userService;
        private readonly IBlobService _blobService;
        public HomeController(IThunderService thunderService,
            IUpsRateService upsRateService, UserManager<ApplicationUser> userManager, IMailService mailService,
             IUserService userService, IBlobService blobService)
        {

            _thunderService = thunderService;
            _upsRateService = upsRateService;
            _userManager = userManager;
            _mailService = mailService;
            _userService = userService;
            _blobService = blobService;
        }
        [AllowAnonymous]
        public async Task<IActionResult> IndexAsync()
        {
            //var client = new HttpClient();
            //var request = new HttpRequestMessage(HttpMethod.Get, "https://api64.ipify.org?format=json");
            //var response = await client.SendAsync(request);
            //response.EnsureSuccessStatusCode();
            //Console.WriteLine(await response.Content.ReadAsStringAsync());
            //var client = new HttpClient();
            //var request = new HttpRequestMessage(HttpMethod.Get,
            //    "https://shipster.org/api/order/8718b34f-a89a-115d-6ddf-f8fbe57c1c47/file");
            //request.Headers.Add("X-Api-Auth", "64cf7549-13e8-6081-b22f-ccd8bf1bdfff");
            //var response = await client.SendAsync(request);
            //// response.EnsureSuccessStatusCode();
            //var result = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(result);

            
            return View();
        }
        [HttpGet]
        public ReturnAddress getUserAddress()
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userAddress = _thunderService.GetUserAddress(uid);
            return userAddress;
        }
        [HttpGet]
        public UserDetails getUserDeets()
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userDeets = _thunderService.GetUserDeets(uid);
            return userDeets;
        }
        [Authorize]
        public IActionResult Dashboard()
        {
        
            return View();
        }

        [HttpPost]
        public List<RateDTO> requestRates(NewRate quickRate)
        {
            var quickRateDTO = _upsRateService.GetQuickRatesAsync(quickRate);
            var rates = quickRateDTO.Result.Rates;
            return rates;
        }
        [Authorize]
        public string getUID()
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return uid;
        }
    
       
        [HttpPost]
        public async Task<IActionResult> GetUserBalance()
        {
            try
            {
                var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userBalance = await _userService.GetUserBalance(uid);
               
                return Ok(userBalance/100); // Return the decimal value without the dollar sign
            }
            catch (Exception ex)
            {
                // Handle the exception here
                // For example:
                // logger.LogError(ex, "Error occurred while retrieving user balance.");
                return StatusCode(500, "An error occurred while retrieving user balance."); // Return an appropriate error response
            }
        }

        /// <summary>
        ///                 ADD THE OGPRICE, PERCENTSAVED ETC. TO LABELDETAILS FROM RATEDTO, 
        ///                 MAYBE AFTER USER PURCHASES. 
        ///                 
        /// </summary>
        /// <param name="upsOrder"></param>
        /// <returns></returns>

        [HttpPost]
        public async Task<IActionResult> GetFullRates(UpsOrderDetails upsOrder)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var deets = await _userService.GetUserDeetsAsync(uid);
           // var emailClaim = User.FindFirst(ClaimTypes.Email);
            upsOrder.FromEmail = deets.Email;
            upsOrder.UserName = deets.FullName;
            upsOrder.Uid = uid;

            try
            {
                FullRateDTO result = await _upsRateService.GetFullRatesAsync(upsOrder);

                _thunderService.AddOrder(upsOrder);

                result.UpsOrderDetailsId = _thunderService.GetUpsOrderDetailsId(uid);
                if (result.UpsOrderDetailsId == null)
                {
                    throw new Exception("Failed to retrieve UpsOrderDetailsId");
                }
                //List<RateDTO> ratesList = new List<RateDTO>();
                //ratesList = result.rates;
                //ratesList.Add(result.selectedrate); // this adds the selectedrate from result.selectedrate back to result.rates;
                //await _thunderService.CreateAndSaveRateCosts((int)result.UpsOrderDetailsId, ratesList);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing the request.", details = ex.InnerException?.Message ?? ex.Message });
            }
        }

        //public List<LabelDetail> getLabelDetails()
        //{
        //    var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    var list = _thunderService.getLabelDetails(uid);
        //    return list;
        //}      
        
        //public List<LabelDetail> getUnfinishedOrders()
        //{
        //    var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    var list = _thunderService.getUnfinishedOrders(uid);
        //    return list;
        //}
        public async Task<IActionResult> DownloadLabel(string orderID)
        {
            try
            {
                return await _thunderService.getLabel(orderID, false);
            }
            catch (Exception ex)
            {
                // Handle exceptions and return an appropriate error view or message
                return View("Error", new ErrorViewModel { ErrorMessage = $"An error occurred while downloading the label: {ex.Message}" });
            }
        }

        public async Task<IActionResult> ViewLabel(string orderID)
        {
            try
            {
                return await _thunderService.getLabel(orderID, true);
            }
            catch (Exception ex)
            {
                // Handle exceptions and return an appropriate error view or message
                return View("Error", new ErrorViewModel { ErrorMessage = $"An error occurred while viewing the label: {ex.Message}" });
            }
        }
        public IActionResult Register()
        {
            return Redirect("~/Identity/Account/Register");
        }
        public IActionResult Login()
        {
            return View("Login");
        }
        public IActionResult Rates()
        {
            return View();
        }
      
        public IActionResult Privacy()
        {
            return View();
        }

      
            [Route("/Home/Error/{statusCode?}")]
            public IActionResult Error(int? statusCode = null)
            {
                var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();
                var exception = feature?.Error;

                ViewData["Exception"] = exception;
                ViewData["StatusCode"] = statusCode;

                return View();
            }
        
    }
}