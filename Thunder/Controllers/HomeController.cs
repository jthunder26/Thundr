using EllipticCurve.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using NuGet.Protocol;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using Thunder.Data;
using Thunder.Models;
using Thunder.Services;

namespace Thunder.Controllers
{

    /// <summary>
   
    /// 
    /// #dec880
    /// #ff347d
    /// #827ded
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
        private readonly ILogger<HomeController> _logger;
        private readonly IThunderService _thunderService;
        private readonly IUpsRateService _upsRateService;
        private readonly IMailService _mailService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserService _userService;
        public HomeController(ILogger<HomeController> logger, IThunderService thunderService,
            IUpsRateService upsRateService, UserManager<ApplicationUser> userManager, IMailService mailService,
             IUserService userService)
        {
            _logger = logger;
            _thunderService = thunderService;
            _upsRateService = upsRateService;
            _userManager = userManager;
            _mailService = mailService;
            _userService = userService;
        }
        [AllowAnonymous]
        public IActionResult Index()
        {
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

        [HttpPost]
        public async Task<IActionResult> GetFullRates(UpsOrderDetails upsOrder)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
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
                List<RateDTO> ratesList = new List<RateDTO>();
                ratesList = result.rates;
                ratesList.Add(result.selectedrate); 
                await _thunderService.CreateAndSaveRateCosts((int)result.UpsOrderDetailsId, ratesList);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing the request.", details = ex.InnerException?.Message ?? ex.Message });
            }
        }

        public List<LabelDetails> getLabelDetails()
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var list = _thunderService.getLabelDetails(uid);
            return list;
        }      
        
        public List<UnfinishedLabel> getUnfinishedOrders()
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var list = _thunderService.getUnfinishedOrders(uid);
            return list;
        }
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
        public async Task<IActionResult> Rates()
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