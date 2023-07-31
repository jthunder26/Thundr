using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;
using System.Configuration;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using Thunder.Data;
using Thunder.Models;
using Azure.Storage.Blobs.Models;
using System.IO;
using System.Runtime.CompilerServices;
using EllipticCurve.Utils;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Azure.Security.KeyVault.Secrets;
using SendGrid.Helpers.Mail;
using Stripe;

namespace Thunder.Services
{
    public interface IThunderService
    {
        void SaveReturnAddress(ReturnAddress returnAddress);
        Task<HttpResponseMessage> CreateAIOLabelAsync(int labelId);
        Task<HttpResponseMessage> CreateShipsterLabelAsync(int labelId);
        //Task<ChargeValidationResult> ValidateChargeAsync(int labelId, long amountCaptured, string stripeCustomerId, string serviceClass);
        UserDetails GetUserDeets(string uid);
        ReturnAddress GetUserAddress(string uid);
        LabelDetails getLabelDetails(string uid);
        //Task TestShipAsync();
        UpsOrderDetails getUnfinishedLabel(int labelId, string uid);
        OrderDTO AdminGetOrderDetails(int labelId);
        Task<FileStreamResult> getLabel(string orderID, bool view = false);
        void UpdateAddress(string uid, ReturnAddress address);
        void AddOrder(UpsOrderDetails order);
        void AddBulkOrder(BulkRateOrderDetails order);
        int GetUpsOrderDetailsId(string uid);
        void UpdateOrder(long amount, int labelId, string serviceClass, long charged, int ogPrice, string percentSaved);
        Task CreateAndSaveRateCosts(int upsOrderDetailsId, List<RateDTO> rates);
        int GetLabelCost(int labelId, string serviceClass);
        bool UpdateUnfinishedOrder(int labelId, string serviceClass, string uid);
        Task DuplicateOrderAsync(int labelId);
    }

    public class ThunderService : IThunderService
    {
        private readonly ApplicationDbContext _db;
        private readonly IBlobService _blobService;
        private readonly IUserService _userService;
        private readonly string _aioApiKeySecret;
        private readonly string _shipsterApiKeySecret;
        private static readonly Dictionary<string, string> shipsterClasses = new Dictionary<string, string>
        {
            { "ups_ground", "48d32fbd-0034-46ff-9b7a-046f3f5b640f" },
            { "ups_next_day_air", "f8952c0f-3467-4a90-ae7b-a52840254218" },
            { "ups_next_day_air_early", "deac3347-10ab-412d-b577-4b90d7566bd6" },
            { "ups_next_day_air_saturday", "f9a64e11-a254-4b16-96d8-444c6fcfa5ed" },
            { "ups_2nd_day_air", "7ab79f8d-d687-4c7e-91b1-d35bcff2ab42" },
            { "ups_3_day_select", "a7ae8133-7a45-4581-b1b2-caca53ab95e2" }
        };

        public ThunderService(ApplicationDbContext db, string aioSecret, string shipsterSecret, IBlobService blobservice, IUserService userService, ILogger<ThunderService> logger)
        {
            _db = db;
            _blobService = blobservice;
            _userService = userService;
           
            _aioApiKeySecret = aioSecret;
            
            
            _shipsterApiKeySecret = shipsterSecret;
        }

        public async Task DuplicateOrderAsync(int labelId)
        {
           
            try
            {
                // Fetch the existing order and label details
                var existingOrder = _db.UpsOrderDetails
                    .Include(o => o.UnfinishedLabel)
                    .FirstOrDefault(o => o.LabelId == labelId);

                if (existingOrder == null)
                {
                    throw new Exception("Order not found");
                }

                var existingLabelDetail = existingOrder.UnfinishedLabel;
                var newOrder = new UpsOrderDetails
                {
                    Uid = existingOrder.Uid,
                    FromEmail = existingOrder.FromEmail,
                    ToEmail = existingOrder.ToEmail,
                    FromName = existingOrder.FromName,
                    FromCompany = existingOrder.FromCompany,
                    FromPhone = existingOrder.FromPhone,
                    FromZip = existingOrder.FromZip,
                    FromAddress1 = existingOrder.FromAddress1,
                    FromAddress2 = existingOrder.FromAddress2,
                    FromCity = existingOrder.FromCity,
                    FromState = existingOrder.FromState,
                    ToName = existingOrder.ToName,
                    ToCompany = existingOrder.ToCompany,
                    ToPhone = existingOrder.ToPhone,
                    ToZip = existingOrder.ToZip,
                    ToAddress1 = existingOrder.ToAddress1,
                    ToAddress2 = existingOrder.ToAddress2,
                    ToCity = existingOrder.ToCity,
                    ToState = existingOrder.ToState,
                    OurPrice = existingOrder.OurPrice,
                    Weight = existingOrder.Weight,
                    Length = existingOrder.Length,
                    Width = existingOrder.Width,
                    Height = existingOrder.Height,
                    checkedOut = 0,
                    UserName = existingOrder.UserName
                };

                _db.UpsOrderDetails.Add(newOrder);
                await _db.SaveChangesAsync();

                // Create a new label detail with the existing label detail's data
                var newLabelDetail = new LabelDetail
                {
                    LabelId = newOrder.LabelId, // The new order's ID
                    Uid = existingLabelDetail.Uid,
                    LabelName = existingLabelDetail.LabelName,
                    FromEmail = existingLabelDetail.FromEmail,
                    Status = 0,
                    DateCreated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    FullName = existingLabelDetail.FullName,
                    CarrierName = existingLabelDetail.CarrierName,
                    OrderId = existingLabelDetail.OrderId,
                    LabelService = existingLabelDetail.LabelService,
                    UpsOrderDetails = newOrder // The new order
                };

                _db.LabelDetail.Add(newLabelDetail);
                await _db.SaveChangesAsync();
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;  // Re-throw the exception if you still want it to bubble up
            }
            // Create a new order with the existing order's data
           
        }


        public async Task<HttpResponseMessage> CreateShipsterLabelAsync(int labelId)
        {
            var UpsOrderDetails = _db.UpsOrderDetails.FirstOrDefault(x => x.LabelId == labelId);
            var labelDetails = _db.LabelDetail.FirstOrDefault(x => x.LabelId == labelId);
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://shipster.org/api/order");
            request.Headers.Add("X-Api-Auth", _shipsterApiKeySecret);

            if (string.IsNullOrEmpty(UpsOrderDetails.FromPhone))
            {
                UpsOrderDetails.FromPhone = GenerateRandomPhoneNumber();
            }
            if (string.IsNullOrEmpty(UpsOrderDetails.ToPhone))
            {
                UpsOrderDetails.ToPhone = GenerateRandomPhoneNumber();
            }

            var collection = new List<KeyValuePair<string, string>>();
            collection.Add(new KeyValuePair<string, string>("FromCountry", "US"));
            collection.Add(new KeyValuePair<string, string>("ToCountry", "US"));
            collection.Add(new KeyValuePair<string, string>("FromName", UpsOrderDetails.FromName));
            collection.Add(new KeyValuePair<string, string>("FromCompany", UpsOrderDetails.FromCompany));
            collection.Add(new KeyValuePair<string, string>("FromPhone", UpsOrderDetails.FromPhone)); //if empty, generate random number
            collection.Add(new KeyValuePair<string, string>("FromZip", UpsOrderDetails.FromZip));
            collection.Add(new KeyValuePair<string, string>("FromStreet", UpsOrderDetails.FromAddress1));
            collection.Add(new KeyValuePair<string, string>("FromStreet2", UpsOrderDetails.FromAddress2));
            collection.Add(new KeyValuePair<string, string>("FromCity", UpsOrderDetails.FromCity));
            collection.Add(new KeyValuePair<string, string>("FromState", UpsOrderDetails.FromState));

            collection.Add(new KeyValuePair<string, string>("ToName", UpsOrderDetails.ToName));
            collection.Add(new KeyValuePair<string, string>("ToCompany", UpsOrderDetails.ToCompany));
            collection.Add(new KeyValuePair<string, string>("ToPhone", UpsOrderDetails.ToPhone));
            collection.Add(new KeyValuePair<string, string>("ToZip", UpsOrderDetails.ToZip));
            collection.Add(new KeyValuePair<string, string>("ToStreet", UpsOrderDetails.ToAddress1));
            collection.Add(new KeyValuePair<string, string>("ToStreet2", UpsOrderDetails.ToAddress2));
            collection.Add(new KeyValuePair<string, string>("ToCity", UpsOrderDetails.ToCity));
            collection.Add(new KeyValuePair<string, string>("ToState", UpsOrderDetails.ToState));

            collection.Add(new KeyValuePair<string, string>("Weight", UpsOrderDetails.Weight.ToString()));
            collection.Add(new KeyValuePair<string, string>("Length", UpsOrderDetails.Length.ToString()));
            collection.Add(new KeyValuePair<string, string>("Width", UpsOrderDetails.Width.ToString()));
            collection.Add(new KeyValuePair<string, string>("Height", UpsOrderDetails.Height.ToString()));
            collection.Add(new KeyValuePair<string, string>("DisableValidate", "true"));
            string shipsterClassId;
            if (UpsOrderDetails.Class == "7deff37b-5900-430c-9335-dabe871bc271")
            {
                shipsterClassId = UpsOrderDetails.Class;
            }
            else
            {
                shipsterClasses.TryGetValue(UpsOrderDetails.Class, out shipsterClassId);

            }
            collection.Add(new KeyValuePair<string, string>("Type", shipsterClassId));


            var content = new FormUrlEncodedContent(collection);
            request.Content = content;

            try
            {
                var response = await client.SendAsync(request);
                //response = {StatusCode: 200, ReasonPhrase: 'OK', Version: 1.1, Content: System.Net.Http.HttpConnectionResponseContent, Headers:\r\n{\r\n  Date: Sat, 08 Apr 2023 00:57:31 GMT\r\n  Transfer-Encoding: chunked\r\n  Connection: keep-alive\r\n  Vary: Accept-Encoding\r\n  Strict-Transport-Security: max-age=31536000\r\n  X-Frame-Options: SAMEORIGIN\r\n  X-XSS-Protection: 1; mode=block\r\n  X-Content-Type-Options: nosniff\r\n  CF-Cache-Status: DYNAMIC\r\n  Report-To: {\"endpoints\":[{\"url\":\"https:\\/\\/a.nel.cloudflare.com\\/report\\/v3?s=dloTtye7MAdOXx42XrShE%2FU%2BRjYVVyBN11MtoHVP9qmDFMV9mFPsYDIiCYx53Z8e3AHIGZ6eun89GGfxyWmNx11r%2BN8PqA5piIwyTNV4Mz11OD74Zt024XFlJczhDZsGjNhudZY%3D\"}],\"group\":\"cf-nel\",\"max_age\":604800}\r\n  NEL: {\"success_fraction\":0,\"report_to\":\"cf-nel\",\"max_age\":604800}\r\n  Server: cloudflare\r\n  CF-RAY: 7b468e7cdbfee976-DFW\r\n  Alt-Svc: h3=\":443\"; ma=86400, h3-29=\":443\"; ma=86400\r\n  Content-Type: application/json\r\n}}

                var responseContent = await response.Content.ReadAsStringAsync();
                //responseContent = {"Success":false,"Error":"You do not have enough balance","Data":[]}
                //var responseContent = "{\"Success\":true,\"Error\":\"\",\"Data\":{\"Order\":{\"ID\":\"8718b34f-a89a-115d-6ddf-f8fbe57c1c47\",\"User\":\"e0ff1b2a-7ce6-a267-4cb5-8d99adaa3342\",\"Type\":\"48d32fbd-0034-46ff-9b7a-046f3f5b640f\",\"Weight\":2,\"Price\":10,\"Status\":2,\"ExternalID\":\"\",\"Tracking\":\"1Z30E7540358676501\",\"Notes\":\"\",\"Length\":1,\"Width\":1,\"Height\":1,\"Description\":\"0\",\"Reference1\":\"0\",\"Reference2\":\"0\",\"CustomsPrice\":0,\"CustomsDescription\":\"0\",\"FromCountry\":\"US\",\"FromName\":\"Juandizzy\",\"FromCompany\":\"Thunda\",\"FromPhone\":\"1231233213\",\"FromStreet\":\"1750 Sky Lark Lane\",\"FromStreet2\":\"\",\"FromCity\":\"Houston\",\"FromState\":\"Houston\",\"FromZip\":\"77056\",\"ToCountry\":\"US\",\"ToName\":\"Jane\",\"ToCompany\":\"\",\"ToPhone\":\"5555301066\",\"ToStreet\":\"1242 Wakefield Drive\",\"ToStreet2\":\"\",\"ToCity\":\"Houston\",\"ToState\":\"TX\",\"ToZip\":\"77018\",\"ShopifyID\":\"\",\"ShopifyFulfillment\":\"\",\"Batch\":\"\",\"BatchIndex\":0,\"Modified\":1686188064,\"Added\":1686188063,\"TypeName\":\"UPS Ground\",\"TypeObject\":{\"ID\":\"48d32fbd-0034-46ff-9b7a-046f3f5b640f\",\"Handler\":\"LabelGenerator\",\"ExternalID\":\"UPS\",\"ExternalClass\":\"3\",\"Name\":\"UPS Ground\",\"Courier\":\"UPS\",\"International\":false,\"AltFields\":false,\"ExtraFields\":true,\"Validate\":false,\"HasDimensions\":true,\"IsCanada\":false,\"MaxWeight\":120,\"WeightUnit\":\"lbs\",\"WeightDecimal\":false,\"TrackLink\":\"https:\\/\\/www.ups.com\\/track?loc=en_US&requester=ST&tracknum=\",\"Enabled\":true,\"MaxWeightFormatted\":\"120 lbs\",\"OriginalPrices\":[{\"From\":1,\"To\":120,\"Price\":10,\"ID\":\"48d32fbd-0034-46ff-9b7a-046f3f5b640f\",\"FromFormatted\":\"1 lbs\",\"ToFormatted\":\"120 lbs\",\"PriceFormatted\":\"$10.00\"}],\"UserPrices\":0,\"Prices\":[{\"From\":1,\"To\":120,\"Price\":10,\"ID\":\"48d32fbd-0034-46ff-9b7a-046f3f5b640f\",\"FromFormatted\":\"1 lbs\",\"ToFormatted\":\"120 lbs\",\"PriceFormatted\":\"$10.00\"}]},\"WeightUnit\":\"lbs\",\"Username\":\"jthunder\",\"PriceFormatted\":\"$10.00\",\"StatusName\":\"Done\",\"StatusColor\":\"success\",\"FromFormatted\":\"Juandizzy Houston 77056\",\"ToFormatted\":\"Jane TX 77018\",\"Cancellable\":false,\"Downloadable\":true,\"Duplicatable\":true,\"Refundable\":true,\"WeightFormatted\":\"2 lbs\",\"LengthFormatted\":\"1.00in\",\"WidthFormatted\":\"1.00in\",\"HeightFormatted\":\"1.00in\",\"CustomsPriceFormatted\":\"$0.00\",\"TrackLink\":\"https:\\/\\/www.ups.com\\/track?loc=en_US&requester=ST&tracknum=1Z30E7540358676501\",\"Trackable\":true,\"ModifiedFormatted\":\"06\\/08\\/2023 01:34\",\"AddedFormatted\":\"06\\/08\\/2023 01:34\"}}}";
                var shipsterResponse = JsonConvert.DeserializeObject<ShipsterResponse>(responseContent);
                if (shipsterResponse.Success)
                {

                    labelDetails.OrderId = shipsterResponse.Data.Order.ID;
                    labelDetails.LabelName = shipsterResponse.Data.Order.ToFormatted;
                    labelDetails.FromEmail = UpsOrderDetails.FromEmail;
                    labelDetails.Uid = UpsOrderDetails.Uid;
                    labelDetails.DateCreated = DateTime.Now.ToString();
                    labelDetails.LabelService = 2;

                    labelDetails.UpsOrderDetails = UpsOrderDetails;

                    _db.SaveChanges();
                }

                if (!response.IsSuccessStatusCode || !shipsterResponse.Success)
                {
                    var errorMessage = "";
                    if (!response.IsSuccessStatusCode)
                    {
                        errorMessage = await response.Content.ReadAsStringAsync();
                    }
                    if (shipsterResponse.Success == false)
                    {

                        errorMessage += shipsterResponse.Error;


                    }
                    throw new ApplicationException($"Error creating UPS label: {response.StatusCode}. {errorMessage}");
                }

                return response;
            }
            catch (Exception ex)
            {
                // Here you can log the exception to a logging service, or write it to a file, or just print it.
                // For simplicity, let's just write it to the console.
                Console.WriteLine($"Exception occurred while sending API request: {ex}");

                // You may also want to propagate the exception further up the chain by rethrowing it:
                throw;
            }
        }


        public async Task<HttpResponseMessage> CreateAIOLabelAsync(int labelId)
        {
            var UpsOrderDetails = _db.UpsOrderDetails.FirstOrDefault(x => x.LabelId == labelId);
            if (UpsOrderDetails.Class == "7deff37b-5900-430c-9335-dabe871bc271" || UpsOrderDetails.Class == "ups_next_day_air_saturday")
            {
                // If it does, throw an exception
                throw new ApplicationException("USPS Class, auto AIO redirect to Shipster");
            }
            var labelDetails = _db.LabelDetail.FirstOrDefault(x => x.LabelId == labelId);
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://aio.gg/api/upsv4/order");
            request.Headers.Add("Auth", _aioApiKeySecret);

            if (string.IsNullOrEmpty(UpsOrderDetails.FromPhone))
            {
                UpsOrderDetails.FromPhone = GenerateRandomPhoneNumber();
            }
            if (string.IsNullOrEmpty(UpsOrderDetails.ToPhone))
            {
                UpsOrderDetails.ToPhone = GenerateRandomPhoneNumber();
            }

            var collection = new List<KeyValuePair<string, string>>();
            collection.Add(new KeyValuePair<string, string>("FromName", UpsOrderDetails.FromName));
            collection.Add(new KeyValuePair<string, string>("FromCompany", UpsOrderDetails.FromCompany));
            collection.Add(new KeyValuePair<string, string>("FromPhone", UpsOrderDetails.FromPhone)); //if empty, generate random number
            collection.Add(new KeyValuePair<string, string>("FromZip", UpsOrderDetails.FromZip));
            collection.Add(new KeyValuePair<string, string>("FromAddress", UpsOrderDetails.FromAddress1));
            collection.Add(new KeyValuePair<string, string>("FromAddress2", UpsOrderDetails.FromAddress2));
            collection.Add(new KeyValuePair<string, string>("FromCity", UpsOrderDetails.FromCity));
            collection.Add(new KeyValuePair<string, string>("FromState", UpsOrderDetails.FromState));

            collection.Add(new KeyValuePair<string, string>("ToName", UpsOrderDetails.ToName));
            collection.Add(new KeyValuePair<string, string>("ToCompany", UpsOrderDetails.ToCompany));
            collection.Add(new KeyValuePair<string, string>("ToPhone", UpsOrderDetails.ToPhone));
            collection.Add(new KeyValuePair<string, string>("ToZip", UpsOrderDetails.ToZip));
            collection.Add(new KeyValuePair<string, string>("ToAddress", UpsOrderDetails.ToAddress1));
            collection.Add(new KeyValuePair<string, string>("ToAddress2", UpsOrderDetails.ToAddress2));
            collection.Add(new KeyValuePair<string, string>("ToCity", UpsOrderDetails.ToCity));
            collection.Add(new KeyValuePair<string, string>("ToState", UpsOrderDetails.ToState));

            collection.Add(new KeyValuePair<string, string>("Weight", UpsOrderDetails.Weight.ToString()));
            collection.Add(new KeyValuePair<string, string>("Length", UpsOrderDetails.Length.ToString()));
            collection.Add(new KeyValuePair<string, string>("Width", UpsOrderDetails.Width.ToString()));
            collection.Add(new KeyValuePair<string, string>("Height", UpsOrderDetails.Height.ToString()));
            collection.Add(new KeyValuePair<string, string>("Class", UpsOrderDetails.Class));

            var content = new FormUrlEncodedContent(collection);
            request.Content = content;


           var response = await client.SendAsync(request);
           // response = {StatusCode: 200, ReasonPhrase: 'OK', Version: 1.1, Content: System.Net.Http.HttpConnectionResponseContent, Headers:\r\n{\r\n  Date: Sat, 08 Apr 2023 00:57:31 GMT\r\n  Transfer-Encoding: chunked\r\n  Connection: keep-alive\r\n  Vary: Accept-Encoding\r\n  Strict-Transport-Security: max-age=31536000\r\n  X-Frame-Options: SAMEORIGIN\r\n  X-XSS-Protection: 1; mode=block\r\n  X-Content-Type-Options: nosniff\r\n  CF-Cache-Status: DYNAMIC\r\n  Report-To: {\"endpoints\":[{\"url\":\"https:\\/\\/a.nel.cloudflare.com\\/report\\/v3?s=dloTtye7MAdOXx42XrShE%2FU%2BRjYVVyBN11MtoHVP9qmDFMV9mFPsYDIiCYx53Z8e3AHIGZ6eun89GGfxyWmNx11r%2BN8PqA5piIwyTNV4Mz11OD74Zt024XFlJczhDZsGjNhudZY%3D\"}],\"group\":\"cf-nel\",\"max_age\":604800}\r\n  NEL: {\"success_fraction\":0,\"report_to\":\"cf-nel\",\"max_age\":604800}\r\n  Server: cloudflare\r\n  CF-RAY: 7b468e7cdbfee976-DFW\r\n  Alt-Svc: h3=\":443\"; ma=86400, h3-29=\":443\"; ma=86400\r\n  Content-Type: application/json\r\n}};

            var responseContent = await response.Content.ReadAsStringAsync();
            //var responseContent = "{\"Success\":true,\"Error\":\"\",\"Data\":{\"Order\":{\"ID\":\"96457f8b-6464-440c-13fb-e83c3fdef935\",\"User\":\"4bd0d4f3-11be-6d1a-1989-6928d6dc6631\",\"Price\":4,\"FromName\":\"Emil Bakiyev\",\"FromCompany\":\"\",\"FromPhone\":\"2819058611\",\"FromZip\":\"77077\",\"FromAddress\":\"2345 Bering dr\",\"FromAddress2\":\"435\",\"FromCity\":\"Houston\",\"FromState\":\"TX\",\"ToName\":\"Electronics buyer \",\"ToCompany\":\"\",\"ToPhone\":\"5557679004\",\"ToZip\":\"94080\",\"ToAddress\":\"274 Harbor Way\",\"ToAddress2\":\"\",\"ToCity\":\"South San Francisco\",\"ToState\":\"CA\",\"Class\":\"ups_ground\",\"Weight\":5,\"Track\":\"1Z6Y020X0399773734\",\"Description\":\"0\",\"Refunded\":false,\"RefundDate\":0,\"Modified\":1687282890,\"Added\":1687282890,\"Length\":6,\"Width\":12,\"Height\":12,\"Ref1\":\"0\",\"Ref2\":\"0\",\"Ref3\":\"0\",\"RoutingCode\":null,\"Provider\":\"2\",\"SignatureRequired\":false,\"Delivered\":0,\"Tries\":0,\"CheckSkipped\":0,\"LastChecked\":null,\"SaturdayDelivery\":false,\"HoldByCourier\":0,\"SuspectHoldByCourier\":0,\"Username\":\"jthunder\",\"ClassFormatted\":\"UPS Ground\",\"PriceFormatted\":\"$4.00\",\"FromFormatted\":\"Emil Bakiyev\",\"ToFormatted\":\"Electronics buyer \",\"Refundable\":true,\"Downloadable\":true,\"Duplicatable\":true,\"Status\":2,\"StatusName\":\"Done\",\"WeightFormatted\":\"5 lbs\",\"LengthFormatted\":\"6 inch\",\"WidthFormatted\":\"12 inch\",\"HeightFormatted\":\"12 inch\",\"ModifiedFormatted\":\"06\\/20\\/2023 17:41\",\"AddedFormatted\":\"06\\/20\\/2023 17:41\",\"SignatureRequiredFormatted\":\"No\",\"SaturdayDeliveryFormatted\":\"No\"}}}";
            var createLabelResponse = JsonConvert.DeserializeObject<AIOResponse>(responseContent);
            if (createLabelResponse.Success)
            {

                labelDetails.OrderId = createLabelResponse.Data.Order.ID;
                labelDetails.LabelName = createLabelResponse.Data.Order.ToFormatted;
                labelDetails.FromEmail = UpsOrderDetails.FromEmail;
                labelDetails.Uid = UpsOrderDetails.Uid;
                labelDetails.LabelService = 1;
                labelDetails.DateCreated = DateTime.Now.ToString();

                labelDetails.UpsOrderDetails = UpsOrderDetails;

                _db.SaveChanges();
            }

            //if (!response.IsSuccessStatusCode || !createLabelResponse.Success)
            //{
            //    var errorMessage = "";
            //    if (!response.IsSuccessStatusCode)
            //    {
            //        errorMessage = await response.Content.ReadAsStringAsync();
            //    }
            //    if (createLabelResponse.Success == false)
            //    {

            //        errorMessage += createLabelResponse.Error;


            //    }
            //    throw new ApplicationException($"Error creating UPS label: {response.StatusCode}. {errorMessage}");
            //}

            return response;
        }








        public ReturnAddress GetUserAddress(string uid)
        {
            var address = _db.ReturnAddress.Where(a => a.Uid == uid).FirstOrDefault();
            var jsonAddress = JsonConvert.SerializeObject(address);
            return address;
        }
        public int GetLabelCost(int labelId, string uid)
        {
            var charge = _db.UpsOrderDetails.Where(a => a.LabelId == labelId && a.Uid == uid).FirstOrDefault();

            return int.Parse(charge.TotalAmount.ToString());
        }
      

        public void UpdateAddress(string uid, ReturnAddress address)
        {
            var og = _db.ReturnAddress.Where(a => a.Uid == uid).FirstOrDefault();
            if(og != null)
            {
                _db.Remove(og);
            }
            
            address.Uid = uid;
            _db.Add(address);
            _db.SaveChanges();


        }


        //public async Task TestShipAsync()
        //{
        //    HttpClient client = new HttpClient();
        //    HttpResponseMessage response = await client.GetAsync("https://api64.ipify.org?format=json");
        //    string ipAddress = await response.Content.ReadAsStringAsync();
        //    Console.WriteLine(ipAddress);
        //    var unfinishedLabel = _db.LabelDetail.Where(a =>a.LabelId == 28).FirstOrDefault();
        //    unfinishedLabel.Message = ipAddress;
        //    _db.SaveChanges();
        //    //    using var client = new HttpClient();
        //    //    var request = new HttpRequestMessage(HttpMethod.Get, "https://shipster.org/api/order/8718b34f-a89a-115d-6ddf-f8fbe57c1c47/file");
        //    //    request.Headers.Add("X-Api-Auth", _shipsterApiKeySecret);


        //    //    var response = await client.SendAsync(request);
        //    //    //response = {StatusCode: 200, ReasonPhrase: 'OK', Version: 1.1, Content: System.Net.Http.HttpConnectionResponseContent, Headers:\r\n{\r\n  Date: Sat, 08 Apr 2023 00:57:31 GMT\r\n  Transfer-Encoding: chunked\r\n  Connection: keep-alive\r\n  Vary: Accept-Encoding\r\n  Strict-Transport-Security: max-age=31536000\r\n  X-Frame-Options: SAMEORIGIN\r\n  X-XSS-Protection: 1; mode=block\r\n  X-Content-Type-Options: nosniff\r\n  CF-Cache-Status: DYNAMIC\r\n  Report-To: {\"endpoints\":[{\"url\":\"https:\\/\\/a.nel.cloudflare.com\\/report\\/v3?s=dloTtye7MAdOXx42XrShE%2FU%2BRjYVVyBN11MtoHVP9qmDFMV9mFPsYDIiCYx53Z8e3AHIGZ6eun89GGfxyWmNx11r%2BN8PqA5piIwyTNV4Mz11OD74Zt024XFlJczhDZsGjNhudZY%3D\"}],\"group\":\"cf-nel\",\"max_age\":604800}\r\n  NEL: {\"success_fraction\":0,\"report_to\":\"cf-nel\",\"max_age\":604800}\r\n  Server: cloudflare\r\n  CF-RAY: 7b468e7cdbfee976-DFW\r\n  Alt-Svc: h3=\":443\"; ma=86400, h3-29=\":443\"; ma=86400\r\n  Content-Type: application/json\r\n}}

        //    //    var responseContent = await response.Content.ReadAsStringAsync();
        //    //    var shipsterResponse = JsonConvert.DeserializeObject<ShipsterResponse>(responseContent);
        //}






        public int GetUpsOrderDetailsId(string uid)
        {
            var upsOrderDetail = _db.UpsOrderDetails.Where(a => a.Uid == uid && a.checkedOut == 1).FirstOrDefault();

            var upsOrderDetailsId = int.Parse(upsOrderDetail.LabelId.ToString());

            return upsOrderDetailsId;
        }
        public void UpdateOrder(long amount, int labelId, string serviceClass, long charged, int ogPrice, string percentSaved)
        {
            var uid = _userService.GetCurrentUserId();
            var upsOrder = _db.UpsOrderDetails.Where(a => a.Uid == uid && a.LabelId == labelId).FirstOrDefault();
            var label = _db.LabelDetail.Where(a => a.Uid == uid && a.LabelId == labelId).FirstOrDefault();
            if(upsOrder != null)
            {

                upsOrder.TotalAmount = int.Parse(amount.ToString()); // same as OurPrice
                upsOrder.LabelId = labelId;
                upsOrder.Class = serviceClass;
                upsOrder.TotalCharge = int.Parse(charged.ToString());
                upsOrder.OurPrice = int.Parse(amount.ToString());
                upsOrder.OgPrice = ogPrice;
                upsOrder.PercentSaved = percentSaved;
            }
            _db.SaveChanges();
        } 
        
        public bool UpdateUnfinishedOrder(int labelId, string serviceClass, string uid)
        {
         
           var unfinishedLabel = _db.LabelDetail.Where(a => a.Uid == uid && a.LabelId == labelId).FirstOrDefault();
            if(unfinishedLabel != null)
            {
                unfinishedLabel.Status = 1;
                _db.SaveChanges();
                return true;
            }
            return false;
        }
        public async Task CreateAndSaveRateCosts(int upsOrderDetailsId, List<RateDTO> rates)
        {
            foreach (var rate in rates)
            {
                // Check if a RateCosts record with the same LabelId and serviceClass exists
                var existingRateCost = _db.RateCosts.FirstOrDefault(rc => rc.LabelId == upsOrderDetailsId && rc.serviceClass == rate.serviceClass);
                if (existingRateCost != null)
                {
                    // Update the TotalCost
                    existingRateCost.TotalCost = rate.exactCost;
                    existingRateCost.TotalCharge = 0;
                }
                else
                {
                    // Create a new RateCosts record
                    RateCosts rateCost = new RateCosts

                    {
                        LabelId = upsOrderDetailsId,
                        serviceClass = rate.serviceClass,
                        TotalCost = rate.exactCost
                    };
                    _db.RateCosts.Add(rateCost);
                }
            }

            await _db.SaveChangesAsync();
        }

        public void AddOrder(UpsOrderDetails upsOrder)
        {
           var orders = _db.UpsOrderDetails.Where(x => x.Uid == upsOrder.Uid);
            var labelDetails = _db.LabelDetail.Where(x => x.Uid == upsOrder.Uid);
            //both of these for eaches just set the checkedout and status = to 0
            foreach (var order in orders)
            {
                order.checkedOut = 0;
            }
          
            foreach (var order in labelDetails)
            {
                if (order.Status < 3)
                {
                    order.Status = 0;
                }
                
            }

            // Check if an entry with the same LabelId already exists in UpsOrderDetails
            var existingUpsOrder = _db.UpsOrderDetails.FirstOrDefault(u => u.LabelId == upsOrder.LabelId);
            var existingLabelDetails = _db.LabelDetail.FirstOrDefault(u => u.LabelId == upsOrder.LabelId);

            if (existingUpsOrder != null)
            {
                upsOrder.checkedOut = 1;
                // Update the existing entry
                _db.Entry(existingUpsOrder).CurrentValues.SetValues(upsOrder);
            }
            else
            {
                // Create a new entry
                upsOrder.checkedOut = 1;
                _db.UpsOrderDetails.Add(upsOrder);
                _db.SaveChanges(); // Save changes to get the generated LabelId
            }

            // Check if an entry with the same LabelId already exists in UnfinishedLabel
         

            if (existingLabelDetails != null)
            {
                // Update the existing entry
                _db.Entry(existingLabelDetails).CurrentValues.SetValues(new LabelDetail
                {
                    LabelId = upsOrder.LabelId,
                    Uid = upsOrder.Uid,
                    LabelName = upsOrder.ToName,
                    FromEmail = upsOrder.FromEmail,
                    DateCreated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = 1
                });
            }
            else
            {
                // Create a new entry
                _db.LabelDetail.Add(new LabelDetail
                {
                    LabelId = upsOrder.LabelId,
                    Uid = upsOrder.Uid,
                    FullName = upsOrder.UserName,
                    LabelName = upsOrder.ToName,
                    FromEmail = upsOrder.FromEmail,
                    DateCreated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = 1
                });

            }

            _db.SaveChanges();
        }
        public void AddBulkOrder(BulkRateOrderDetails orderDetails)
        {
            try
            {
                var orders = _db.UpsOrderDetails.Where(x => x.Uid == orderDetails.Uid);
                var labelDetails = _db.LabelDetail.Where(x => x.Uid == orderDetails.Uid);
                int bulkID = 0;
                //both of these for eaches just set the checkedout and status = to 0
                foreach (var order in orders)
                {
                    order.checkedOut = 0;
                }

                foreach (var order in labelDetails)
                {
                    if (order.Status < 3)
                    {
                        order.Status = 0;
                    }
                }

                foreach (var order in orderDetails.OrderDetails)
                {
                    // Create a new entry for UpsOrderDetails
                    order.checkedOut = 1;
                    order.Uid = orderDetails.Uid;
                    _db.UpsOrderDetails.Add(order);

                    // Save changes to generate LabelId
                    _db.SaveChanges();

                    // Get the generated LabelId
                    int generatedLabelId = order.LabelId;

                    if (bulkID == 0)
                    {
                        bulkID = generatedLabelId;
                    }

                    order.BulkId = bulkID;

                    // Now we can create the LabelDetail
                    _db.LabelDetail.Add(new LabelDetail
                    {
                        LabelId = generatedLabelId,
                        BulkId = bulkID,
                        Uid = order.Uid,
                        FullName = order.UserName,
                        LabelName = order.ToName,
                        FromEmail = order.FromEmail,
                        DateCreated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Status = 1
                    });

                    // Save changes after each order addition
                    _db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                // Handle the exception according to your needs
                // For example, you could log the error or rethrow it
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        ////public void AddBulkOrder(BulkRateOrderDetails orderDetails)
        //{
        //    var orders = _db.UpsOrderDetails.Where(x => x.Uid == orderDetails.Uid);
        //    var labelDetails = _db.LabelDetail.Where(x => x.Uid == orderDetails.Uid);
        //    int bulkID = 0;
        //    //both of these for eaches just set the checkedout and status = to 0
        //    foreach (var order in orders)
        //    {
        //        order.checkedOut = 0;
        //    }

        //    foreach (var order in labelDetails)
        //    {
        //        if (order.Status < 3)
        //        {
        //            order.Status = 0;
        //        }

        //    }
        //    foreach (var order in orderDetails.OrderDetails)
        //    {
        //        // Create a new entry for UpsOrderDetails
        //        order.checkedOut = 1;
        //        order.Uid = orderDetails.Uid;
        //        _db.UpsOrderDetails.Add(order);
        //        _db.SaveChanges(); // Save changes to get the generated LabelId

        //        // Save the generated LabelId
        //        int generatedLabelId = order.LabelId;

        //        if (bulkID == 0)
        //        {
        //            bulkID = generatedLabelId;
        //        }

        //        order.BulkId = bulkID;
        //        _db.SaveChanges();

        //        // Now we can create the LabelDetail
        //        _db.LabelDetail.Add(new LabelDetail
        //        {
        //            LabelId = generatedLabelId,
        //            BulkId = bulkID,
        //            Uid = order.Uid,
        //            FullName = order.UserName,
        //            LabelName = order.ToName,
        //            FromEmail = order.FromEmail,
        //            DateCreated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        //            Status = 1
        //        });
        //        _db.SaveChanges();

        //    }
        //}

        public UserDetails GetUserDeets(string uid)
        {
            var user = _db.Users.FindAsync(uid);
            UserDetails userDeets = new UserDetails();
            userDeets.FullName = user.Result.FullName;
            userDeets.Email = user.Result.Email;
            userDeets.Phone = user.Result.PhoneNumber;
            return userDeets;
        }
       
        public async Task<FileStreamResult> getLabel(string orderID, bool view = false)
        {
            var stream = await _blobService.GetPdfFromBlobAsync(orderID);

            // Set the content disposition based on the view parameter
            var contentDisposition = view ? "inline" : "attachment";
            var fileName = "ShippingLabel.pdf";

            var response = new FileStreamResult(stream, "application/pdf")
            {
                FileDownloadName = view ? null : fileName
            };

            return response;
        }

        public void SaveReturnAddress(ReturnAddress returnAddress)
        {
            _db.Add(returnAddress);
            _db.SaveChanges();
        }
        public LabelDetails getLabelDetails(string uid)
        {
            // The ToList() operation is performed immediately here,
            // thus allowing subsequent operations on the in-memory list rather than the database.
            var orders = _db.LabelDetail.Where(c => c.Uid == uid);

                //.OrderByDescending(x => x.DateCreated);

            var finishedOrders = orders.Where(x => x.Status == 3).OrderByDescending(c => c.DateCreated);
            var unfinishedOrders = orders.Where(x => x.Status < 3).OrderByDescending(c => c.DateCreated);

            LabelDetails labelDetails = new LabelDetails();
            labelDetails.FinishedOrders = finishedOrders.ToList();
            labelDetails.UnfinishedOrders = unfinishedOrders.ToList();
            return labelDetails;
        }



        //public List<LabelDetail> getUnfinishedOrders(string uid)
        //{
        //    var list = _db.LabelDetail.Where(c => c.Uid == uid && c.Status < 3)
        //                        .OrderByDescending(c => c.DateCreated);


        //    return list.ToList();
        //}

        public UpsOrderDetails getUnfinishedLabel(int labelId, string uid)
        {
            var upsOrderDetails = _db.UpsOrderDetails
                                   .Where(x => x.LabelId == labelId && x.Uid == uid)
                                   .FirstOrDefault();


            return upsOrderDetails;
        }
          public OrderDTO AdminGetOrderDetails(int labelId)
        {
            var orderDetails = _db.UpsOrderDetails
                                   .Where(x => x.LabelId == labelId)
                                   .FirstOrDefault();
            var labelDetails = _db.LabelDetail.Where(z => z.LabelId == labelId).FirstOrDefault(); 
           
            OrderDTO order = new OrderDTO();
            order.LabelId = orderDetails.LabelId;
            order.UserName = orderDetails.UserName;
            order.OrderMessage = labelDetails.Message;
            order.ErrorMessage = labelDetails.ErrorMsg;
            order.Status = labelDetails.Status.ToString();
            order.LabelServiceAttempts = "AIO attempts: " + labelDetails.AIO_Attempt.ToString() + " - Shipster Attempts: " + labelDetails.Shipster_Attempt.ToString();
           
            order.FromEmail = orderDetails.FromEmail;
            order.FromName = orderDetails.FromName;
            order.FromCompany = orderDetails.FromCompany;
            order.FromAddress1 = orderDetails.FromAddress1;
            order.FromAddress2 = orderDetails.FromAddress2;
            order.FromCity = orderDetails.FromCity;
            order.FromState = orderDetails.FromState;
            order.FromZip = orderDetails.FromZip;
            order.FromPhone = orderDetails.FromPhone;
           
            order.ToEmail = orderDetails.ToEmail;
            order.ToName = orderDetails.ToName;
            order.ToCompany = orderDetails.ToCompany;
            order.ToAddress1 = orderDetails.ToAddress1;
            order.ToAddress2 = orderDetails.ToAddress2;
            order.ToCity = orderDetails.ToCity;
            order.ToState = orderDetails.ToState;
            order.ToZip = orderDetails.ToZip;
            order.ToPhone = orderDetails.ToPhone;

            order.Length = orderDetails.Length;
            order.Width = orderDetails.Width;
            order.Height = orderDetails.Height;
            order.Weight = orderDetails.Weight;
            order.ServiceClass = orderDetails.Class;
            order.OGPrice = orderDetails.OgPrice.ToString();
            order.PercentSaved = orderDetails.PercentSaved;
            order.OurPrice = orderDetails.OurPrice.ToString();
            order.TotalCharge = orderDetails.TotalCharge.ToString();
            return order;
        }

     

        private string GenerateRandomPhoneNumber()
        {
            var random = new Random();
            return $"555{random.Next(100, 999)}{random.Next(1000, 9999)}";
        }

    }
    public class ItemizedChargesConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ItemizedCharges[]);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.Object)
            {
                var singleItem = token.ToObject<ItemizedCharges>(serializer);
                return new[] { singleItem };
            }
            else if (token.Type == JTokenType.Array)
            {
                return token.ToObject<ItemizedCharges[]>(serializer);
            }
            else
            {
                throw new JsonSerializationException("Unexpected token type for ItemizedCharges");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecessary because CanWrite is false. The type will skip the converter.");
        }

        public override bool CanWrite => false;
    }
}
