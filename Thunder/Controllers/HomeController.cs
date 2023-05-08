using EllipticCurve.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using NuGet.Protocol;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using Thunder.Models;
using Thunder.Services;

namespace Thunder.Controllers
{

    /// <summary>
    /// Items left to complete before hosting:
    /// 
    /// Webflow:
    /// 
    /// Ensure Logout button is visible in Wide View - Done
    /// Create Best Value tag for rates. - DONE
    /// Make the Fastest tag text same color and size. Rates and Ship Page - done
    /// The Zipcode input In account goes small when in Large desktop
    /// mini nav menu hamburger icon and  background color is gray when opening. data-nav-menu-open
    /// Orders page - gradient image is wylin at 590px
    /// Visual Studios: 
    /// 
    /// Create Email Templates for Account Creation, Payment Confirmation, Password Reset. 
    /// Add auto complete for Ship From as well. 
    /// Create Blob Storage to store labels
    /// Transfer Domain to cloudflare -- In Progress
    /// Add the auto scroll 
    /// Check error from createLabel() to Aio, if Success is false and Error is not over balance issues/format
    /// --> Switch API's
    /// Items to be added in the future:
    /// 
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
        public HomeController(ILogger<HomeController> logger, IThunderService thunderService,
            IUpsRateService upsRateService)
        {
            _logger = logger;
            _thunderService = thunderService;
            _upsRateService = upsRateService;
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
       
        [HttpPost]
        public async Task<IActionResult> GetFullRates(UpsOrderDetails fullRate)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            fullRate.Uid = uid;
            try
            {
                FullRateDTO result = await _upsRateService.GetFullRatesAsync(fullRate);

                //THIS IS STILL ADDING MULTIPLE ORDERS
                _thunderService.AddOrder(fullRate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception here, if needed

                return StatusCode(500, new { message = "An error occurred while processing the request.", details = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> makeTheLabel(CreateUpsLabel UpsOrderDetails)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var response = await _thunderService.CreateUPSLabelAsync(UpsOrderDetails, uid);

                // Handle success case, e.g. return a success message or redirect to another page
                return Json(new { redirectToUrl = Url.Action("Ship", "Dashboard") });
            }
            catch (ApplicationException ex)
            {
                // Handle the error case, e.g. return an error message or show an error view
                ModelState.AddModelError(string.Empty, ex.Message);
                return Json(new { redirectToUrl = Url.Action("Ship", "Dashboard") });
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}