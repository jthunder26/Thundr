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

namespace Thunder.Services
{
    public interface IThunderService
    {
        void SaveReturnAddress(ReturnAddress returnAddress);
        Task<HttpResponseMessage> CreateAIOLabelAsync(int labelId);
        //Task<ChargeValidationResult> ValidateChargeAsync(int labelId, long amountCaptured, string stripeCustomerId, string serviceClass);
        UserDetails GetUserDeets(string uid);
        ReturnAddress GetUserAddress(string uid);
        List<LabelDetails> getLabelDetails(string uid);
        List<UnfinishedLabel> getUnfinishedOrders(string uid);
        UpsOrderDetails getUnfinishedLabel(int labelId, string uid);
        Task<FileStreamResult> getLabel(string orderID, bool view = false);
        void UpdateAddress(string uid, ReturnAddress address);
        void AddOrder(UpsOrderDetails order);
        int GetUpsOrderDetailsId(string uid);
        void UpdateOrder(long amount, int labelId, string serviceClass);
        Task CreateAndSaveRateCosts(int upsOrderDetailsId, List<RateDTO> rates);
        int GetLabelCost(int labelId, string serviceClass);
        void UpdateUnfinishedOrder(int labelId, string serviceClass);
    }

    public class ThunderService : IThunderService
    {
        private readonly ApplicationDbContext _db;
        private readonly IBlobService _blobService;
        private readonly IUserService _userService;
        private readonly ILogger<ThunderService> _logger;
        public ThunderService(ApplicationDbContext db, IBlobService blobservice, IUserService userService, ILogger<ThunderService> logger)
        {
            _db = db;
            _blobService = blobservice;
            _userService = userService;
            _logger = logger; // add this line
        }
        public ReturnAddress GetUserAddress(string uid)
        {
            var address = _db.ReturnAddress.Where(a => a.Uid == uid).FirstOrDefault();
            var jsonAddress = JsonConvert.SerializeObject(address);
            return address;
        }
        public int GetLabelCost(int labelId, string serviceClass)
        {
            var charge = _db.RateCosts.Where(a => a.LabelId == labelId && a.serviceClass == serviceClass).FirstOrDefault();
            
            return charge.TotalCost;
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
        //Okay ur not able to get the UPSOrderLabelID, now what?????










        public int GetUpsOrderDetailsId(string uid)
        {
            var upsOrderDetail = _db.UpsOrderDetails.Where(a => a.Uid == uid && a.checkedOut == 1).FirstOrDefault();

            var upsOrderDetailsId = int.Parse(upsOrderDetail.LabelId.ToString());

            return upsOrderDetailsId;
        }
        public void UpdateOrder(long amount, int labelId, string serviceClass)
        {
            var uid = _userService.GetCurrentUserId();
            var upsOrder = _db.UpsOrderDetails.Where(a => a.Uid == uid && a.LabelId == labelId).FirstOrDefault();
            if(upsOrder != null)
            {

                upsOrder.TotalAmount = int.Parse(amount.ToString());
                upsOrder.LabelId = labelId;
                upsOrder.Class = serviceClass;


            }
            _db.SaveChanges();
        } 
        
        public void UpdateUnfinishedOrder(int labelId, string serviceClass)
        {
            var uid = _userService.GetCurrentUserId();
            var unfinishedLabel = _db.UnfinishedLabel.Where(a => a.Uid == uid && a.LabelId == labelId).FirstOrDefault();
            if(unfinishedLabel != null)
            {
                unfinishedLabel.Status = 1;

            }
            _db.SaveChanges();
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
            foreach (var order in orders)
            {
                order.checkedOut = 0;
            }

            // Check if an entry with the same LabelId already exists in UpsOrderDetails
            var existingUpsOrder = _db.UpsOrderDetails.FirstOrDefault(u => u.LabelId == upsOrder.LabelId);

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
            var existingUnfinishedLabel = _db.UnfinishedLabel.FirstOrDefault(u => u.LabelId == upsOrder.LabelId);

            if (existingUnfinishedLabel != null)
            {
                // Update the existing entry
                _db.Entry(existingUnfinishedLabel).CurrentValues.SetValues(new UnfinishedLabel
                {
                    LabelId = upsOrder.LabelId,
                    Uid = upsOrder.Uid,
                    LabelName = upsOrder.ToName,
                    FromEmail = upsOrder.FromEmail,
                    DateCreated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            else
            {
                // Create a new entry
                _db.UnfinishedLabel.Add(new UnfinishedLabel
                {
                    LabelId = upsOrder.LabelId,
                    Uid = upsOrder.Uid,
                    LabelName = upsOrder.ToName,
                    FromEmail = upsOrder.FromEmail,
                    DateCreated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }

            _db.SaveChanges();
        }


        public UserDetails GetUserDeets(string uid)
        {
            var user = _db.Users.FindAsync(uid);
            UserDetails userDeets = new UserDetails();
            userDeets.FullName = user.Result.FullName;
            userDeets.Email = user.Result.Email;
            userDeets.Phone = user.Result.PhoneNumber;
            return userDeets;
        }
        public async Task SaveLabelToBlobStorage(string orderID)
        {
            // Code for saving the PDF to Blob Storage using your existing blob service implementation
            await _blobService.SavePdfToBlobAsync(orderID);
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
        public List<LabelDetails> getLabelDetails(string uid)
        {
            var list = _db.LabelDetails.Where(c => c.Uid == uid)
                .OrderByDescending(x => x.DateCreated);


            return list.ToList();
        }

     
        public List<UnfinishedLabel> getUnfinishedOrders(string uid)
        {
            var list = _db.UnfinishedLabel.Where(c => c.Uid == uid)
                                .OrderByDescending(c => c.DateCreated);


            return list.ToList();
        }
        
        public UpsOrderDetails getUnfinishedLabel(int labelId, string uid)
        {
            var upsOrderDetails = _db.UpsOrderDetails
                                   .Where(x => x.LabelId == labelId && x.Uid == uid)
                                   .FirstOrDefault();


            return upsOrderDetails;
        }

        //public async Task<ChargeValidationResult> ValidateChargeAsync(int labelId, long amountCaptured, string stripeCustomerId, string serviceClass)
        //{
            

        //        try
        //        {
        //        var UpsOrderDetails = _db.UpsOrderDetails.FirstOrDefault(x => x.LabelId == labelId && x.Class == serviceClass);
        //        var rc = _db.RateCosts.FirstOrDefault(x => x.LabelId == labelId && x.serviceClass == serviceClass);
        //        var response = await _userService.UpdateUserBalanceByCustomerIdAsync(stripeCustomerId, amountCaptured);
        //            if (response.Success)
        //            {
        //                var chargeCustomerResult = await _userService.ChargeCustomer(stripeCustomerId, rc.TotalCost);
        //                if (!chargeCustomerResult.Succeeded)
        //                {
        //                    _logger.LogError($"Error while charging customer: {string.Join(", ", chargeCustomerResult.Errors.Select(x => x.Description))}");
        //                    return new ChargeValidationResult { Success = false, Message = $"Error while charging customer: {string.Join(", ", chargeCustomerResult.Errors.Select(x => x.Description))}" };
        //                }
        //            }
                   
        //            return new ChargeValidationResult { Success = true, LabelId = labelId };
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, $"An error occurred when creating the label for labelId: {labelId}");
        //            return new ChargeValidationResult { Success = false, Message = ex.Message+" Label Id:"+labelId };
        //        }
          

        //    // Handle case where TotalAmount != TotalCost
        //    return new ChargeValidationResult { Success = false, Message = "AmountCaptured does not equal TotalCost" };
        //}


        public async Task<HttpResponseMessage> CreateAIOLabelAsync(int labelId)
        {
            var UpsOrderDetails = _db.UpsOrderDetails.FirstOrDefault(x => x.LabelId == labelId);
         
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://aio.gg/api/upsv4/order");
            request.Headers.Add("Auth", "62527fb0-85be-5781-8fa8-8f6a12a0a0fe");

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
            collection.Add(new KeyValuePair<string, string>("FromState", UpsOrderDetails.FromCity));

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
            //response = {StatusCode: 200, ReasonPhrase: 'OK', Version: 1.1, Content: System.Net.Http.HttpConnectionResponseContent, Headers:\r\n{\r\n  Date: Sat, 08 Apr 2023 00:57:31 GMT\r\n  Transfer-Encoding: chunked\r\n  Connection: keep-alive\r\n  Vary: Accept-Encoding\r\n  Strict-Transport-Security: max-age=31536000\r\n  X-Frame-Options: SAMEORIGIN\r\n  X-XSS-Protection: 1; mode=block\r\n  X-Content-Type-Options: nosniff\r\n  CF-Cache-Status: DYNAMIC\r\n  Report-To: {\"endpoints\":[{\"url\":\"https:\\/\\/a.nel.cloudflare.com\\/report\\/v3?s=dloTtye7MAdOXx42XrShE%2FU%2BRjYVVyBN11MtoHVP9qmDFMV9mFPsYDIiCYx53Z8e3AHIGZ6eun89GGfxyWmNx11r%2BN8PqA5piIwyTNV4Mz11OD74Zt024XFlJczhDZsGjNhudZY%3D\"}],\"group\":\"cf-nel\",\"max_age\":604800}\r\n  NEL: {\"success_fraction\":0,\"report_to\":\"cf-nel\",\"max_age\":604800}\r\n  Server: cloudflare\r\n  CF-RAY: 7b468e7cdbfee976-DFW\r\n  Alt-Svc: h3=\":443\"; ma=86400, h3-29=\":443\"; ma=86400\r\n  Content-Type: application/json\r\n}}

            var responseContent = await response.Content.ReadAsStringAsync();
            //responseContent = {"Success":false,"Error":"You do not have enough balance","Data":[]}
            var createLabelResponse = JsonConvert.DeserializeObject<CreateLabelResponse>(responseContent);
            if (createLabelResponse.Success)
            {
                LabelDetails labelDetails = new LabelDetails();
                labelDetails.OrderId = createLabelResponse.Data.Order.ID;
                labelDetails.LabelName = createLabelResponse.Data.Order.ToFormatted;
                labelDetails.Email = UpsOrderDetails.FromEmail;
                labelDetails.Uid = UpsOrderDetails.Uid;
                labelDetails.DateCreated = DateTime.Now.ToString();
                _db.Add(labelDetails);
            }
            _db.SaveChanges();
            if (!response.IsSuccessStatusCode || !createLabelResponse.Success)
            {
                var errorMessage = "";
                if (!response.IsSuccessStatusCode)
                {
                    errorMessage = await response.Content.ReadAsStringAsync();
                }
                if (createLabelResponse.Success == false)
                {

                    errorMessage += createLabelResponse.Error;


                }
                throw new ApplicationException($"Error creating UPS label: {response.StatusCode}. {errorMessage}");
            }

            return response;
        }

        private string GenerateRandomPhoneNumber()
        {
            var random = new Random();
            return $"555{random.Next(1000, 9999)}{random.Next(1000, 9999)}";
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
