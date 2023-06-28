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
      
        private readonly IThunderService _thunderService;
        private readonly IUpsRateService _upsRateService;
        private readonly IMailService _mailService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserService _userService;
        private readonly IBlobService _blobService;
        public DashboardController(IThunderService thunderService,
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
        public async void UpdateAddress(ReturnAddress address)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _thunderService.UpdateAddress(uid, address);
           
        }
        [HttpPost]
        public List<RateDTO> requestRates(NewRate quickRate)
        {
            var quickRateDTO = _upsRateService.GetQuickRatesAsync(quickRate);
            var rates = quickRateDTO.Result.Rates;
            Console.WriteLine();
            return rates;
        }

      
        [Authorize]
        public LabelDetails getLabelDetails()
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var labelDetails = _thunderService.getLabelDetails(uid);
            return labelDetails;
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