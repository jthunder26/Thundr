using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Thunder.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IThunderService _thunderService;
        private readonly IUpsRateService _upsRateService;
        private readonly IMailService _mailService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserService _userService; 
        public DashboardController(ILogger<HomeController> logger, IThunderService thunderService,
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

        //public void Store(Items items)
        //{

        //}


        [HttpGet]
        public ReturnAddress getUserAddress()
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userAddress = _thunderService.GetUserAddress(uid);
            return userAddress;
        }
        [Authorize]
        public async Task<IActionResult> DashboardAsync()
        {
            
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
           
            return View("Orders");
        }
        [Authorize]
        public IActionResult PaymentProcessing()
        {

            return View();
        }
        [Authorize]
        public IActionResult Account()
        {

            return View();
        } 
        [Authorize]
        public async Task<IActionResult> ShipAsync()
        {
            var result = await _userService.FindByStripeCustomerIdAsync("cus_NwXJcN02PW5vlO");
            if (result.Success)
            {
                var updated = await _userService.UpdateUserBalance(result.uid, 20000);

                if (updated)
                {
                   var labelCost = _thunderService.GetLabelCost(14, "ups_next_day_air_early");
                    var result2 = await _userService.ChargeUserBalance(result.uid, labelCost);
                    if(result2.Success)
                    {
                        _thunderService.UpdateUnfinishedOrder(14, "ups_next_day_air_early");
                    }
                }
            }
               
                 

            return View();
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetUnfinishedLabel(int labelId)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Fetch the UpsOrderDetails entry corresponding to the received LabelId
            var upsOrderDetails = _thunderService.getUnfinishedLabel(labelId, uid);

            // Pass the UpsOrderDetails entry to the Ship view
            return Json(upsOrderDetails);
        }

        [Authorize]
        public IActionResult Orders()
        {

            return View();
        }
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> ValidatePasswordAsync(string password)
        {
            var userClaim = this.User;
            var user = await _userManager.GetUserAsync(userClaim);
            if (user == null)
            {

                return Json(new { status = "false" });
            }

            var result = await _userManager.CheckPasswordAsync(user, password);
            return Json(new { result });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateUserInfoAsync(string fullName, string email, string phone)
        {
            var userClaim = this.User;
            var user = await _userManager.GetUserAsync(userClaim);
            if (user == null)
            {

                return Json(new { status = "false" });
            }
            user.FullName = fullName;
            user.Email = email;
            user.UserName = email;
            user.PhoneNumber = phone;
            var result = await _userManager.UpdateAsync(user);


            return Json(new { redirectToUrl = Url.Action("Account", "Dashboard") });
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateAddress(ReturnAddress address)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _thunderService.UpdateAddress(uid, address);
            return Json(new { redirectToUrl = Url.Action("Account", "Dashboard") });
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
          
            try
            {
                FullRateDTO result = await _upsRateService.GetFullRatesAsync(fullRate);
                _thunderService.AddOrder(fullRate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception here, if needed
                return StatusCode(500, new { message = "An error occurred while processing the request.", details = ex.Message });
            }
        }
        //[Authorize]
        //[HttpPost]
        //public async Task<IActionResult> makeTheLabel(CreateUpsLabel UpsOrderDetails)
        //{
        //    var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    try
        //    {
        //        var response = await _thunderService.CreateUPSLabelAsync(UpsOrderDetails, uid);

        //        // Handle success case, e.g. return a success message or redirect to another page
        //        return RedirectToAction("Dashboard");
        //    }
        //    catch (ApplicationException ex)
        //    {
        //        // Handle the error case, e.g. return an error message or show an error view
        //        ModelState.AddModelError(string.Empty, ex.Message);
        //        return View("Dashboard", UpsOrderDetails);
        //    }
        //}
        public List<LabelDetails> getLabelDetails()
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var list = _thunderService.getLabelDetails(uid);
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
        [Authorize]
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