using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Stripe;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Thunder.Data;
using Thunder.Models;
using Thunder.Services;
using Thunder.Migrations;

namespace Thunder.Areas.Identity.Pages.Account
{
    //form should be no bigger than 550px
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
       
        private readonly IEmailSender _emailSender;
        private readonly IThunderService _thunderService;
        private readonly IMailService _mailService;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;
        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            IThunderService thunderService,
            IMailService mailService,
            IConfiguration configuration,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _emailSender = emailSender;
            _thunderService = thunderService;
            _mailService = mailService;
            _configuration = configuration;
            _roleManager = roleManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
      
        public bool IsReturnAddress { get; set; }
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }
            [Required]
            [StringLength(255, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
            [Display(Name = "FullName")]
            public string FullName { get; set; }

            [Display(Name = "PhoneNumber"), Required]
            public string PhoneNumber { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

        }


        public async Task OnGetAsync(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            //ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            //returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (ModelState.IsValid)
            {
                var user = CreateUser();
                user.FullName = Input.FullName;
                user.PhoneNumber = Input.PhoneNumber;
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var stripeSecretKey = _configuration["StripeApiKey"];

                StripeConfiguration.ApiKey = stripeSecretKey;
                var customerOptions = new CustomerCreateOptions
                {
                    Email = Input.Email
                };


                var customerService = new CustomerService();
                var stripeCustomer = customerService.Create(customerOptions);

                // Retrieve the customer ID from the Stripe customer object
                string customerId = stripeCustomer.Id;
                user.StripeCustomerId = customerId;




                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    //address.Uid = user.Id;
                    //_thunderService.SaveReturnAddress(address);



                    //_logger.LogInformation("User created a new account with password.");
                    await _userManager.AddToRoleAsync(user, "User");
                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    //await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                    //    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    var content = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">\r\n<html data-editor-version=\"2\" class=\"sg-campaigns\" xmlns=\"http://www.w3.org/1999/xhtml\">\r\n    <head>\r\n      <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">\r\n      <meta name=\"viewport\" content=\"width=device-width, initial-scale=1, minimum-scale=1, maximum-scale=1\">\r\n      <!--[if !mso]><!-->\r\n      <meta http-equiv=\"X-UA-Compatible\" content=\"IE=Edge\">\r\n      <!--<![endif]-->\r\n      <!--[if (gte mso 9)|(IE)]>\r\n      <xml>\r\n        <o:OfficeDocumentSettings>\r\n          <o:AllowPNG/>\r\n          <o:PixelsPerInch>96</o:PixelsPerInch>\r\n        </o:OfficeDocumentSettings>\r\n      </xml>\r\n      <![endif]-->\r\n      <!--[if (gte mso 9)|(IE)]>\r\n  <style type=\"text/css\">\r\n    body {width: 600px;margin: 0 auto;}\r\n    table {border-collapse: collapse;}\r\n    table, td {mso-table-lspace: 0pt;mso-table-rspace: 0pt;}\r\n    img {-ms-interpolation-mode: bicubic;}\r\n  </style>\r\n<![endif]-->\r\n      <style type=\"text/css\">\r\n    body, p, div {\r\n      font-family: inherit;\r\n      font-size: 14px;\r\n    }\r\n    body {\r\n      color: #000000;\r\n    }\r\n    body a {\r\n      color: #1188E6;\r\n      text-decoration: none;\r\n    }\r\n    p { margin: 0; padding: 0; }\r\n    table.wrapper {\r\n      width:100% !important;\r\n      table-layout: fixed;\r\n      -webkit-font-smoothing: antialiased;\r\n      -webkit-text-size-adjust: 100%;\r\n      -moz-text-size-adjust: 100%;\r\n      -ms-text-size-adjust: 100%;\r\n    }\r\n    img.max-width {\r\n      max-width: 100% !important;\r\n    }\r\n    .column.of-2 {\r\n      width: 50%;\r\n    }\r\n    .column.of-3 {\r\n      width: 33.333%;\r\n    }\r\n    .column.of-4 {\r\n      width: 25%;\r\n    }\r\n    ul ul ul ul  {\r\n      list-style-type: disc !important;\r\n    }\r\n    ol ol {\r\n      list-style-type: lower-roman !important;\r\n    }\r\n    ol ol ol {\r\n      list-style-type: lower-latin !important;\r\n    }\r\n    ol ol ol ol {\r\n      list-style-type: decimal !important;\r\n    }\r\n    @media screen and (max-width:480px) {\r\n      .preheader .rightColumnContent,\r\n      .footer .rightColumnContent {\r\n        text-align: left !important;\r\n      }\r\n      .preheader .rightColumnContent div,\r\n      .preheader .rightColumnContent span,\r\n      .footer .rightColumnContent div,\r\n      .footer .rightColumnContent span {\r\n        text-align: left !important;\r\n      }\r\n      .preheader .rightColumnContent,\r\n      .preheader .leftColumnContent {\r\n        font-size: 80% !important;\r\n        padding: 5px 0;\r\n      }\r\n      table.wrapper-mobile {\r\n        width: 100% !important;\r\n        table-layout: fixed;\r\n      }\r\n      img.max-width {\r\n        height: auto !important;\r\n        max-width: 100% !important;\r\n      }\r\n      a.bulletproof-button {\r\n        display: block !important;\r\n        width: auto !important;\r\n        font-size: 80%;\r\n        padding-left: 0 !important;\r\n        padding-right: 0 !important;\r\n      }\r\n      .columns {\r\n        width: 100% !important;\r\n      }\r\n      .column {\r\n        display: block !important;\r\n        width: 100% !important;\r\n        padding-left: 0 !important;\r\n        padding-right: 0 !important;\r\n        margin-left: 0 !important;\r\n        margin-right: 0 !important;\r\n      }\r\n      .social-icon-column {\r\n        display: inline-block !important;\r\n      }\r\n    }\r\n  </style>\r\n      <!--user entered Head Start--><link href=\"https://fonts.googleapis.com/css?family=Fira+Sans+Condensed&display=swap\" rel=\"stylesheet\"><style>\r\n    body {font-family: 'Fira Sans Condensed', sans-serif;}\r\n</style><!--End Head user entered-->\r\n    </head>\r\n    <body>\r\n      <center class=\"wrapper\" data-link-color=\"#1188E6\" data-body-style=\"font-size:14px; font-family:inherit; color:#000000; background-color:#f0f0f0;\">\r\n        <div class=\"webkit\">\r\n          <table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" width=\"100%\" class=\"wrapper\" bgcolor=\"#f0f0f0\">\r\n            <tr>\r\n              <td valign=\"top\" bgcolor=\"#f0f0f0\" width=\"100%\">\r\n                <table width=\"100%\" role=\"content-container\" class=\"outer\" align=\"center\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n                  <tr>\r\n                    <td width=\"100%\">\r\n                      <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n                        <tr>\r\n                          <td>\r\n                            <!--[if mso]>\r\n    <center>\r\n    <table><tr><td width=\"600\">\r\n  <![endif]-->\r\n                                    <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:100%; max-width:600px;\" align=\"center\">\r\n                                      <tr>\r\n                                        <td role=\"modules-container\" style=\"padding:0px 0px 0px 0px; color:#000000; text-align:left;\" bgcolor=\"#FFFFFF\" width=\"100%\" align=\"left\"><table class=\"module preheader preheader-hide\" role=\"module\" data-type=\"preheader\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"display: none !important; mso-hide: all; visibility: hidden; opacity: 0; color: transparent; height: 0; width: 0;\">\r\n    <tr>\r\n      <td role=\"module-content\">\r\n        <p></p>\r\n      </td>\r\n    </tr>\r\n  </table><table class=\"module\" role=\"module\" data-type=\"divider\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"table-layout: fixed;\" data-muid=\"c5a3c312-4d9d-44ff-9fce-6a8003caeeca.1\">\r\n    <tbody>\r\n      <tr>\r\n        <td style=\"padding:0px 20px 0px 20px;\" role=\"module-content\" height=\"100%\" valign=\"top\" bgcolor=\"#000000\">\r\n          <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" align=\"center\" width=\"100%\" height=\"4px\" style=\"line-height:4px; font-size:4px;\">\r\n            <tbody>\r\n              <tr>\r\n                <td style=\"padding:0px 0px 4px 0px;\" bgcolor=\"#b6b6b6\"></td>\r\n              </tr>\r\n            </tbody>\r\n          </table>\r\n        </td>\r\n      </tr>\r\n    </tbody>\r\n  </table><table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" align=\"center\" width=\"100%\" role=\"module\" data-type=\"columns\" style=\"padding:5px 0px 30px 5px;\" bgcolor=\"#ffffff\" data-distribution=\"1\">\r\n    <tbody>\r\n      <tr role=\"module-content\">\r\n        <td height=\"100%\" valign=\"top\"><table width=\"575\" style=\"width:575px; border-spacing:0; border-collapse:collapse; margin:0px 10px 0px 10px;\" cellpadding=\"0\" cellspacing=\"0\" align=\"left\" border=\"0\" bgcolor=\"\" class=\"column column-0\">\r\n      <tbody>\r\n        <tr>\r\n          <td style=\"padding:0px;margin:0px;border-spacing:0;\"><table class=\"wrapper\" role=\"module\" data-type=\"image\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"table-layout: fixed;\" data-muid=\"a169501c-69eb-4f62-ad93-ac0150abdf03\">\r\n    <tbody>\r\n      <tr>\r\n        <td style=\"font-size:6px; line-height:10px; padding:10px 0px 0px 0px;\" valign=\"top\" align=\"left\">\r\n          <img class=\"max-width\" border=\"0\" style=\"display:block; color:#000000; text-decoration:none; font-family:Helvetica, arial, sans-serif; font-size:16px;\" width=\"175\" alt=\"\" data-proportionally-constrained=\"true\" data-responsive=\"false\" src=\"http://cdn.mcauto-images-production.sendgrid.net/95d0584d6aae9916/a8fed700-316b-406d-9070-649a449b25b5/924x294.png\" height=\"56\">\r\n        </td>\r\n      </tr>\r\n    </tbody>\r\n  </table></td>\r\n        </tr>\r\n      </tbody>\r\n    </table></td>\r\n      </tr>\r\n    </tbody>\r\n  </table><table class=\"module\" role=\"module\" data-type=\"text\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"table-layout: fixed;\" data-muid=\"080768f5-7b16-4756-a254-88cfe5138113\" data-mc-module-version=\"2019-10-22\">\r\n    <tbody>\r\n      <tr>\r\n        <td style=\"padding:30px 30px 18px 30px; line-height:36px; text-align:inherit; background-color:#ffffff;\" height=\"100%\" valign=\"top\" bgcolor=\"#ffffff\" role=\"module-content\"><div><div style=\"font-family: inherit; text-align: center\"><span style=\"font-family: &quot;lucida sans unicode&quot;, &quot;lucida grande&quot;, sans-serif; font-size: 30px; color: #000000\">Your account has been successfully created!</span></div><div></div></div></td>\r\n      </tr>\r\n    </tbody>\r\n  </table><table class=\"module\" role=\"module\" data-type=\"text\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"table-layout: fixed;\" data-muid=\"cddc0490-36ba-4b27-8682-87881dfbcc14\" data-mc-module-version=\"2019-10-22\">\r\n    <tbody>\r\n      <tr>\r\n        <td style=\"padding:18px 30px 18px 30px; line-height:22px; text-align:inherit; background-color:#ffffff;\" height=\"100%\" valign=\"top\" bgcolor=\"#ffffff\" role=\"module-content\"><div><div style=\"font-family: inherit; text-align: inherit\"><span style=\"font-size: 15px; color: #000000\"><strong>Hi, " + user.FullName + "!</strong></span></div>\r\n<div style=\"font-family: inherit; text-align: inherit\"><br></div>\r\n<div style=\"font-family: inherit; text-align: inherit\"><span style=\"font-size: 15px; color: #000000\">We are thrilled to welcome you to the ThundrShip community!&nbsp;</span></div>\r\n<div style=\"font-family: inherit; text-align: inherit\"><br></div>\r\n<div style=\"font-family: inherit; text-align: inherit\"><span style=\"font-size: 15px; color: #000000\">Your account has been successfully created and you are now part of a network of businesses and individuals leveraging the power of our state-of-the-art shipping tools and solutions.&nbsp;</span></div>\r\n<div style=\"font-family: inherit; text-align: inherit\"><br></div>\r\n<div style=\"font-family: inherit; text-align: inherit\"><span style=\"font-size: 15px; color: #000000\">We strive to provide the best shipping experience with the most competitive rates from </span><span style=\"font-size: 15px; color: #000000\"><strong>UPS </strong></span><span style=\"font-size: 15px; color: #000000\">and </span><span style=\"font-size: 15px; color: #000000\"><strong>USPS</strong></span><span style=\"font-size: 15px; color: #000000\">. If you have any questions or need assistance with anything else, don't hesitate to reach out to our support team (</span><span style=\"font-size: 15px; color: #000000\"><strong>support@thundrship.com</strong></span><span style=\"font-size: 15px; color: #000000\">).<br>\r\n<br>\r\nWe're excited to help you achieve your shipping goals, and we can't wait to see where your business goes from here.<br>\r\n<br>\r\nThank you for choosing ThundrShip.<br>\r\n<br>\r\nHappy shipping!</span></div>\r\n<div style=\"font-family: inherit; text-align: inherit\"><span style=\"font-size: 15px; color: #000000\"><br>\r\n</span><span style=\"font-size: 15px; color: #000000\"><strong>Best,</strong></span><span style=\"font-size: 15px; color: #000000\"><br>\r\n</span><span style=\"font-size: 15px; color: #000000\"><strong>Your ThundrShip Team</strong></span></div><div></div></div></td>\r\n      </tr>\r\n    </tbody>\r\n  </table><table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"module\" data-role=\"module-button\" data-type=\"button\" role=\"module\" style=\"table-layout:fixed;\" width=\"100%\" data-muid=\"cd669415-360a-41a6-b4b4-ca9e149980b3\">\r\n      <tbody>\r\n        <tr>\r\n          <td align=\"center\" bgcolor=\"#ffffff\" class=\"outer-td\" style=\"padding:30px 0px 30px 0px; background-color:#ffffff;\">\r\n            <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"wrapper-mobile\" style=\"text-align:center;\">\r\n              <tbody>\r\n                <tr>\r\n                <td align=\"center\" bgcolor=\"#ffffff\" class=\"inner-td\" style=\"border-radius:6px; font-size:16px; text-align:center; background-color:inherit;\">\r\n                  <a href=\"https://thundrship.com/Dashboard/Ship\" style=\"background-color:#ffffff; border:1px solid #0c0c0c; border-color:#0c0c0c; border-radius:40px; border-width:1px; color:#000000; display:inline-block; font-size:15px; font-weight:normal; letter-spacing:0px; line-height:25px; padding:12px 18px 10px 18px; text-align:center; text-decoration:none; border-style:solid; font-family:inherit; width:168px;\" target=\"_blank\">Start Shipping!</a>\r\n                </td>\r\n                </tr>\r\n              </tbody>\r\n            </table>\r\n          </td>\r\n        </tr>\r\n      </tbody>\r\n    </table><table class=\"module\" role=\"module\" data-type=\"text\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"table-layout: fixed;\" data-muid=\"eb301547-da19-441f-80a1-81e1b56e64ad\" data-mc-module-version=\"2019-10-22\">\r\n    <tbody>\r\n      <tr>\r\n        <td style=\"padding:30px 0px 18px 0px; line-height:22px; text-align:inherit; background-color:#f0f0f0;\" height=\"100%\" valign=\"top\" bgcolor=\"#f0f0f0\" role=\"module-content\"><div><div style=\"font-family: inherit; text-align: center\"><span style=\"font-family: inherit; font-size: 24px\"><strong>Upcoming Features</strong></span></div><div></div></div></td>\r\n      </tr>\r\n    </tbody>\r\n  </table><table class=\"module\" role=\"module\" data-type=\"divider\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"table-layout: fixed;\" data-muid=\"c5a3c312-4d9d-44ff-9fce-6a8003caeeca.1.2\">\r\n    <tbody>\r\n      <tr>\r\n        <td style=\"padding:0px 20px 0px 20px;\" role=\"module-content\" height=\"100%\" valign=\"top\" bgcolor=\"#000000\">\r\n          <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" align=\"center\" width=\"100%\" height=\"3px\" style=\"line-height:3px; font-size:3px;\">\r\n            <tbody>\r\n              <tr>\r\n                <td style=\"padding:0px 0px 3px 0px;\" bgcolor=\"#B6B6B6\"></td>\r\n              </tr>\r\n            </tbody>\r\n          </table>\r\n        </td>\r\n      </tr>\r\n    </tbody>\r\n  </table><table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" align=\"center\" width=\"100%\" role=\"module\" data-type=\"columns\" style=\"padding:0px 20px 30px 20px;\" bgcolor=\"\" data-distribution=\"1,1\">\r\n    <tbody>\r\n      <tr role=\"module-content\">\r\n        <td height=\"100%\" valign=\"top\"><table width=\"265\" style=\"width:265px; border-spacing:0; border-collapse:collapse; margin:0px 15px 0px 0px;\" cellpadding=\"0\" cellspacing=\"0\" align=\"left\" border=\"0\" bgcolor=\"\" class=\"column column-0\">\r\n      <tbody>\r\n        <tr>\r\n          <td style=\"padding:0px;margin:0px;border-spacing:0;\"><table class=\"module\" role=\"module\" data-type=\"spacer\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"table-layout: fixed;\" data-muid=\"a45551e7-98d7-40da-889d-a0dc41550c4e\">\r\n    <tbody>\r\n      <tr>\r\n        <td style=\"padding:0px 0px 15px 0px;\" role=\"module-content\" bgcolor=\"\">\r\n        </td>\r\n      </tr>\r\n    </tbody>\r\n  </table><table class=\"wrapper\" role=\"module\" data-type=\"image\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"table-layout: fixed;\" data-muid=\"8ZPkEyRmw35sXLUWrdumXA.1\">\r\n    <tbody>\r\n      <tr>\r\n        <td style=\"font-size:6px; line-height:10px; padding:0px 0px 0px 0px;\" valign=\"top\" align=\"left\">\r\n          <img class=\"max-width\" border=\"0\" style=\"display:block; color:#000000; text-decoration:none; font-family:Helvetica, arial, sans-serif; font-size:16px; max-width:100% !important; width:100%; height:auto !important;\" width=\"265\" alt=\"\" data-proportionally-constrained=\"true\" data-responsive=\"true\" src=\"http://cdn.mcauto-images-production.sendgrid.net/95d0584d6aae9916/3c2b7f7e-0ba6-4057-a32a-e3fa73bc20b7/2000x1191.jpg\">\r\n        </td>\r\n      </tr>\r\n    </tbody>\r\n  </table></td>\r\n        </tr>\r\n      </tbody>\r\n    </table><table width=\"265\" style=\"width:265px; border-spacing:0; border-collapse:collapse; margin:0px 0px 0px 15px;\" cellpadding=\"0\" cellspacing=\"0\" align=\"left\" border=\"0\" bgcolor=\"\" class=\"column column-1\">\r\n      <tbody>\r\n        <tr>\r\n          <td style=\"padding:0px;margin:0px;border-spacing:0;\"><table class=\"module\" role=\"module\" data-type=\"text\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"table-layout: fixed;\" data-muid=\"4vL54iw2MCdgWcxxaCgLhi.1\" data-mc-module-version=\"2019-10-22\">\r\n    <tbody>\r\n      <tr>\r\n        <td style=\"padding:18px 0px 18px 0px; line-height:20px; text-align:inherit; background-color:#ffffff;\" height=\"100%\" valign=\"top\" bgcolor=\"#ffffff\" role=\"module-content\"><div><div style=\"font-family: inherit; text-align: start\"><span style=\"box-sizing: border-box; padding-top: 0px; padding-right: 0px; padding-bottom: 0px; padding-left: 0px; margin-top: 0px; margin-right: 0px; margin-bottom: 0px; margin-left: 0px; font-style: inherit; font-variant-ligatures: inherit; font-variant-caps: inherit; font-variant-numeric: inherit; font-variant-east-asian: inherit; font-variant-alternates: inherit; font-weight: bold; font-stretch: inherit; line-height: inherit; font-family: inherit; font-optical-sizing: inherit; font-kerning: inherit; font-feature-settings: inherit; font-variation-settings: inherit; vertical-align: baseline; border-top-width: 0px; border-right-width: 0px; border-bottom-width: 0px; border-left-width: 0px; border-top-style: initial; border-right-style: initial; border-bottom-style: initial; border-left-style: initial; border-top-color: initial; border-right-color: initial; border-bottom-color: initial; border-left-color: initial; border-image-source: initial; border-image-slice: initial; border-image-width: initial; border-image-outset: initial; border-image-repeat: initial; letter-spacing: normal; orphans: 2; text-align: start; text-indent: 0px; text-transform: none; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; white-space-collapse: preserve; text-wrap: wrap; text-decoration-thickness: initial; text-decoration-style: initial; text-decoration-color: initial; font-size: 18px\">Real-Time Map Tracking!</span></div>\r\n<div style=\"font-family: inherit; text-align: start\"><span style=\"font-size: 15px; color: #282828\">An interactive, real-time tracking map for all your ThundrShip deliveries. No more searching through lengthy text updates or puzzling over obscure location details. You'll be able to visually track your shipments from origin to destination and see every stop along the way!</span></div><div></div></div></td>\r\n      </tr>\r\n    </tbody>\r\n  </table></td>\r\n        </tr>\r\n      </tbody>\r\n    </table></td>\r\n      </tr>\r\n    </tbody>\r\n  </table><table class=\"module\" role=\"module\" data-type=\"divider\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"table-layout: fixed;\" data-muid=\"c5a3c312-4d9d-44ff-9fce-6a8003caeeca.1.1\">\r\n    <tbody>\r\n      <tr>\r\n        <td style=\"padding:0px 20px 0px 20px;\" role=\"module-content\" height=\"100%\" valign=\"top\" bgcolor=\"#4d5171\">\r\n          <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" align=\"center\" width=\"100%\" height=\"3px\" style=\"line-height:3px; font-size:3px;\">\r\n            <tbody>\r\n              <tr>\r\n                <td style=\"padding:0px 0px 3px 0px;\" bgcolor=\"#B6B6B6\"></td>\r\n              </tr>\r\n            </tbody>\r\n          </table>\r\n        </td>\r\n      </tr>\r\n    </tbody>\r\n  </table><table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" align=\"center\" width=\"100%\" role=\"module\" data-type=\"columns\" style=\"padding:0px 20px 0px 20px;\" bgcolor=\"#ffffff\" data-distribution=\"1,1\">\r\n    <tbody>\r\n      <tr role=\"module-content\">\r\n        <td height=\"100%\" valign=\"top\"><table width=\"270\" style=\"width:270px; border-spacing:0; border-collapse:collapse; margin:0px 10px 0px 0px;\" cellpadding=\"0\" cellspacing=\"0\" align=\"left\" border=\"0\" bgcolor=\"\" class=\"column column-0\">\r\n      <tbody>\r\n        <tr>\r\n          <td style=\"padding:0px;margin:0px;border-spacing:0;\"><table class=\"module\" role=\"module\" data-type=\"spacer\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"table-layout: fixed;\" data-muid=\"a45551e7-98d7-40da-889d-a0dc41550c4e.1.1\">\r\n    <tbody>\r\n      <tr>\r\n        <td style=\"padding:0px 0px 15px 0px;\" role=\"module-content\" bgcolor=\"\">\r\n        </td>\r\n      </tr>\r\n    </tbody>\r\n  </table><table class=\"wrapper\" role=\"module\" data-type=\"image\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"table-layout: fixed;\" data-muid=\"8ZPkEyRmw35sXLUWrdumXA.2\">\r\n    <tbody>\r\n      <tr>\r\n        <td style=\"font-size:6px; line-height:10px; padding:0px 0px 0px 0px;\" valign=\"top\" align=\"center\">\r\n          <img class=\"max-width\" border=\"0\" style=\"display:block; color:#000000; text-decoration:none; font-family:Helvetica, arial, sans-serif; font-size:16px;\" width=\"239\" height=\"150\" alt=\"\" data-proportionally-constrained=\"true\" data-responsive=\"false\" src=\"http://cdn.mcauto-images-production.sendgrid.net/95d0584d6aae9916/5f87db8a-615c-4135-ab89-2e388b326843/600x377.jpg\">\r\n        </td>\r\n      </tr>\r\n    </tbody>\r\n  </table></td>\r\n        </tr>\r\n      </tbody>\r\n    </table><table width=\"270\" style=\"width:270px; border-spacing:0; border-collapse:collapse; margin:0px 0px 0px 10px;\" cellpadding=\"0\" cellspacing=\"0\" align=\"left\" border=\"0\" bgcolor=\"\" class=\"column column-1\">\r\n      <tbody>\r\n        <tr>\r\n          <td style=\"padding:0px;margin:0px;border-spacing:0;\"><table class=\"module\" role=\"module\" data-type=\"text\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"table-layout: fixed;\" data-muid=\"4vL54iw2MCdgWcxxaCgLhi.2\" data-mc-module-version=\"2019-10-22\">\r\n    <tbody>\r\n      <tr>\r\n        <td style=\"padding:18px 0px 18px 0px; line-height:20px; text-align:inherit; background-color:#ffffff;\" height=\"100%\" valign=\"top\" bgcolor=\"#ffffff\" role=\"module-content\"><div><div style=\"font-family: inherit; text-align: inherit\"><span style=\"color: #000000; font-size: 18px\"><strong>Introducing Bulk Shipping!</strong></span></div>\r\n<div style=\"font-family: inherit; text-align: inherit\"><br></div>\r\n<div style=\"font-family: inherit; text-align: inherit\"><span style=\"font-size: 15px; color: #000000\">For our customers that want to ship a batch of packages! Download our Excel template, fill out the information and upload back to ThundrShip.com for easy batch shipping!</span></div>\r\n<div style=\"font-family: inherit; text-align: inherit\"><br></div><div></div></div></td>\r\n      </tr>\r\n    </tbody>\r\n  </table></td>\r\n        </tr>\r\n      </tbody>\r\n    </table></td>\r\n      </tr>\r\n    </tbody>\r\n  </table><table class=\"module\" role=\"module\" data-type=\"divider\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"table-layout: fixed;\" data-muid=\"c5a3c312-4d9d-44ff-9fce-6a8003caeeca.1.1.1\">\r\n    <tbody>\r\n      <tr>\r\n        <td style=\"padding:0px 20px 0px 20px;\" role=\"module-content\" height=\"100%\" valign=\"top\" bgcolor=\"#4d5171\">\r\n          <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" align=\"center\" width=\"100%\" height=\"3px\" style=\"line-height:3px; font-size:3px;\">\r\n            <tbody>\r\n              <tr>\r\n                <td style=\"padding:0px 0px 3px 0px;\" bgcolor=\"#B6B6B6\"></td>\r\n              </tr>\r\n            </tbody>\r\n          </table>\r\n        </td>\r\n      </tr>\r\n    </tbody>\r\n  </table><table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" align=\"center\" width=\"100%\" role=\"module\" data-type=\"columns\" style=\"padding:10px 20px 10px 20px;\" bgcolor=\"#ffffff\" data-distribution=\"1,1\">\r\n    <tbody>\r\n      <tr role=\"module-content\">\r\n        <td height=\"100%\" valign=\"top\"><table width=\"265\" style=\"width:265px; border-spacing:0; border-collapse:collapse; margin:0px 15px 0px 0px;\" cellpadding=\"0\" cellspacing=\"0\" align=\"left\" border=\"0\" bgcolor=\"\" class=\"column column-0\">\r\n      <tbody>\r\n        <tr>\r\n          <td style=\"padding:0px;margin:0px;border-spacing:0;\"></td>\r\n        </tr>\r\n      </tbody>\r\n    </table><table width=\"265\" style=\"width:265px; border-spacing:0; border-collapse:collapse; margin:0px 0px 0px 15px;\" cellpadding=\"0\" cellspacing=\"0\" align=\"left\" border=\"0\" bgcolor=\"\" class=\"column column-1\">\r\n      <tbody>\r\n        <tr>\r\n          <td style=\"padding:0px;margin:0px;border-spacing:0;\"><table class=\"wrapper\" role=\"module\" data-type=\"image\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"table-layout: fixed;\" data-muid=\"8ZPkEyRmw35sXLUWrdumXA.1.1\">\r\n    <tbody>\r\n      <tr>\r\n        <td style=\"font-size:6px; line-height:10px; padding:0px 0px 0px 0px;\" valign=\"top\" align=\"right\">\r\n          <img class=\"max-width\" border=\"0\" style=\"display:block; color:#000000; text-decoration:none; font-family:Helvetica, arial, sans-serif; font-size:16px;\" width=\"45\" alt=\"\" data-proportionally-constrained=\"true\" data-responsive=\"false\" src=\"http://cdn.mcauto-images-production.sendgrid.net/95d0584d6aae9916/6a8287bb-fe61-4105-bee3-5f088f3e0f06/649x1188.png\" height=\"82\">\r\n        </td>\r\n      </tr>\r\n    </tbody>\r\n  </table></td>\r\n        </tr>\r\n      </tbody>\r\n    </table></td>\r\n      </tr>\r\n    </tbody>\r\n  </table><table class=\"module\" role=\"module\" data-type=\"text\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"table-layout: fixed;\" data-muid=\"4vL54iw2MCdgWcxxaCgLhi.1.1\" data-mc-module-version=\"2019-10-22\">\r\n    <tbody>\r\n      <tr>\r\n        <td style=\"padding:0px 0px 5px 0px; line-height:20px; text-align:inherit;\" height=\"100%\" valign=\"top\" bgcolor=\"\" role=\"module-content\"><div><div style=\"font-family: inherit; text-align: start\"><span style=\"font-size: 15px; color: #c8c8c8\">ThundrShip Inc. 2023</span></div><div></div></div></td>\r\n      </tr>\r\n    </tbody>\r\n  </table><div data-role=\"module-unsubscribe\" class=\"module\" role=\"module\" data-type=\"unsubscribe\" style=\"background-color:#F0F0F0; font-size:12px; line-height:20px; padding:16px 16px 16px 16px; text-align:Center;\" data-muid=\"4e838cf3-9892-4a6d-94d6-170e474d21e5\"><div class=\"Unsubscribe--addressLine\"></div><p style=\"font-size:12px; line-height:20px;\"><a target=\"_blank\" class=\"Unsubscribe--unsubscribeLink zzzzzzz\" href=\"{{{unsubscribe}}}\" style=\"\">Unsubscribe</a> - <a href=\"{{{unsubscribe_preferences}}}\" target=\"_blank\" class=\"Unsubscribe--unsubscribePreferences\" style=\"\">Unsubscribe Preferences</a></p></div></td>\r\n                                      </tr>\r\n                                    </table>\r\n                                    <!--[if mso]>\r\n                                  </td>\r\n                                </tr>\r\n                              </table>\r\n                            </center>\r\n                            <![endif]-->\r\n                          </td>\r\n                        </tr>\r\n                      </table>\r\n                    </td>\r\n                  </tr>\r\n                </table>\r\n              </td>\r\n            </tr>\r\n          </table>\r\n        </div>\r\n      </center>\r\n    </body>\r\n  </html>\r\n";
                    await _mailService.SendEmailAsync(Input.Email, "Welcome to ThundrShip!", content);
                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Ship", "Dashboard");
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

       

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
