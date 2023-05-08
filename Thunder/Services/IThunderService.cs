using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using Thunder.Data;
using Thunder.Models;

namespace Thunder.Services
{
    public interface IThunderService
    {
        void SaveReturnAddress(ReturnAddress returnAddress);
        Task<HttpResponseMessage> CreateUPSLabelAsync(CreateUpsLabel UpsOrderDetails, string uid);
        UserDetails GetUserDeets(string uid);
        ReturnAddress GetUserAddress(string uid);
        List<LabelDetails> getLabelDetails(string uid);
        List<UnfinishedLabel> getUnfinishedOrders(string uid);
        UpsOrderDetails getUnfinishedLabel(int labelId, string uid);
        Task<FileStreamResult> getLabel(string orderID, bool view = false);
        void UpdateAddress(string uid, ReturnAddress address);
        void AddOrder(UpsOrderDetails order);
    }

    public class ThunderService : IThunderService
    {
        private readonly ApplicationDbContext _db;
        public ThunderService(ApplicationDbContext db)
        {
            _db = db;
        }
        public ReturnAddress GetUserAddress(string uid)
        {
            var address = _db.ReturnAddress.Where(a => a.Uid == uid).FirstOrDefault();
            var jsonAddress = JsonConvert.SerializeObject(address);
            return address;
        }

        public void UpdateAddress(string uid, ReturnAddress address)
        {
            var og = _db.ReturnAddress.Where(a => a.Uid == uid).FirstOrDefault();
            _db.Remove(og);
            address.Uid = uid;
            _db.Add(address);
            _db.SaveChanges();


        }
        public void AddOrder(UpsOrderDetails upsOrder)
        {
            // Check if an entry with the same LabelId already exists in UpsOrderDetails
            var existingUpsOrder = _db.UpsOrderDetails.FirstOrDefault(u => u.LabelId == upsOrder.LabelId);

            if (existingUpsOrder != null)
            {
                // Update the existing entry
                _db.Entry(existingUpsOrder).CurrentValues.SetValues(upsOrder);
            }
            else
            {
                // Create a new entry
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




        //public void AddOrder(UpsOrderDetails upsOrder)
        //{
        //    // Add the UpsOrderDetails object to the database
        //    _db.Add(upsOrder);
        //    _db.SaveChanges();

        //    // Create a new UnfinishedLabel object with the corresponding UpsOrderDetails properties
        //    var unfinishedLabel = new UnfinishedLabel
        //    {
        //        LabelId = upsOrder.LabelId,
        //        Uid = upsOrder.Uid,
        //        LabelName = upsOrder.ToName,
        //        FromEmail = upsOrder.FromEmail,
        //        Status = 0, // Or any initial value you'd like to set
        //        DateCreated = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
        //    };

        //    // Add the UnfinishedLabel object to the database and save the changes
        //    _db.Add(unfinishedLabel);
        //    _db.SaveChanges();
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
            using var client = new HttpClient();
            var uri = "https://aio.gg/api/upsv3/order/" + orderID + "/file";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Auth", "c422df81-015d-632d-4a3d-3281c0b4d952");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            MemoryStream ms = new MemoryStream(await response.Content.ReadAsByteArrayAsync());
            var contentDisposition = view ? "inline" : "attachment";
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(contentDisposition)
            {
                FileName = "ShippingLabel.pdf"
            };

            return new FileStreamResult(ms, "application/pdf")
            {
                FileDownloadName = view ? null : "ShippingLabel.pdf"
            };
        }


        public void SaveReturnAddress(ReturnAddress returnAddress)
        {
            _db.Add(returnAddress);
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
        public async Task<HttpResponseMessage> CreateUPSLabelAsync(CreateUpsLabel UpsOrderDetails, string uid)
        {
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://aio.gg/api/upsv3/order");
            request.Headers.Add("Auth", "c422df81-015d-632d-4a3d-3281c0b4d952");

            if (string.IsNullOrEmpty(UpsOrderDetails.order.FromPhone))
            {
                UpsOrderDetails.order.FromPhone = GenerateRandomPhoneNumber();
            }
            if (string.IsNullOrEmpty(UpsOrderDetails.order.ToPhone))
            {
                UpsOrderDetails.order.ToPhone = GenerateRandomPhoneNumber();
            }

            var collection = new List<KeyValuePair<string, string>>();
            collection.Add(new KeyValuePair<string, string>("FromName", UpsOrderDetails.order.FromName));
            collection.Add(new KeyValuePair<string, string>("FromCompany", UpsOrderDetails.order.FromCompany));
            collection.Add(new KeyValuePair<string, string>("FromPhone", UpsOrderDetails.order.FromPhone)); //if empty, generate random number
            collection.Add(new KeyValuePair<string, string>("FromZip", UpsOrderDetails.order.FromZip));
            collection.Add(new KeyValuePair<string, string>("FromAddress", UpsOrderDetails.order.FromAddress1));
            collection.Add(new KeyValuePair<string, string>("FromAddress2", UpsOrderDetails.order.FromAddress2));
            collection.Add(new KeyValuePair<string, string>("FromCity", UpsOrderDetails.order.FromCity));
            collection.Add(new KeyValuePair<string, string>("FromState", UpsOrderDetails.order.FromCity));

            collection.Add(new KeyValuePair<string, string>("ToName", UpsOrderDetails.order.ToName));
            collection.Add(new KeyValuePair<string, string>("ToCompany", UpsOrderDetails.order.ToCompany));
            collection.Add(new KeyValuePair<string, string>("ToPhone", UpsOrderDetails.order.ToPhone));
            collection.Add(new KeyValuePair<string, string>("ToZip", UpsOrderDetails.order.ToZip));
            collection.Add(new KeyValuePair<string, string>("ToAddress", UpsOrderDetails.order.ToAddress1));
            collection.Add(new KeyValuePair<string, string>("ToAddress2", UpsOrderDetails.order.ToAddress2));
            collection.Add(new KeyValuePair<string, string>("ToCity", UpsOrderDetails.order.ToCity));
            collection.Add(new KeyValuePair<string, string>("ToState", UpsOrderDetails.order.ToState));

            collection.Add(new KeyValuePair<string, string>("Weight", UpsOrderDetails.order.Weight.ToString()));
            collection.Add(new KeyValuePair<string, string>("Length", UpsOrderDetails.order.Length.ToString()));
            collection.Add(new KeyValuePair<string, string>("Width", UpsOrderDetails.order.Width.ToString()));
            collection.Add(new KeyValuePair<string, string>("Height", UpsOrderDetails.order.Height.ToString()));
            collection.Add(new KeyValuePair<string, string>("Class", UpsOrderDetails.serviceClass));

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
                labelDetails.Email = UpsOrderDetails.order.FromEmail;
                labelDetails.Uid = uid;
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
