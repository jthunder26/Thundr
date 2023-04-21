using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
    public class ThunderService : IThunderService
    {
        private readonly ApplicationDbContext _db;
        public ThunderService(ApplicationDbContext db)
        {
            _db = db;
        }
        public ReturnAddress GetUserAddress(string uid)
        {
            var address = _db.ReturnAddress.Where(a=>a.Id == uid).FirstOrDefault();
            var jsonAddress = JsonConvert.SerializeObject(address);
            return address;
        }
      
        public void UpdateAddress(string uid, ReturnAddress address)
        {
            var og = _db.ReturnAddress.Where(a => a.Id == uid).FirstOrDefault();
            _db.Remove(og);
            address.Id = uid;
            _db.Add(address);
            _db.SaveChanges();

            
        }

       
        //THEN SET UP EMAIL SERVICE - CONFIGURE DOMAIN - SEND GRID
        //SET UP PASSWORD RESET SERVICE 
       
        public UserDetails GetUserDeets(string uid)
        {
            var user = _db.Users.FindAsync(uid);
            UserDetails userDeets = new UserDetails();
            userDeets.FullName = user.Result.FullName;
            userDeets.Email = user.Result.Email;
            userDeets.Phone = user.Result.PhoneNumber;
            return userDeets;
        } 
       

        //MAKE THE ORDERID DYNAMIC IN THE URL MF
        public async Task<IActionResult> getLabelAsync()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://aio.gg/api/upsv3/order/94fb3553-3040-83e6-ec9f-f7312edfa01e/file");
            request.Headers.Add("Auth", "6b730685-896c-83ba-01e9-e3bf2ea2df38");
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = response.Content;
            MemoryStream ms = new MemoryStream(await response.Content.ReadAsByteArrayAsync());
            return new FileStreamResult(ms, "application/pdf");
        }
        public void SaveUpsLabel(UpsOrder upsOrder)
        {

        }

        public void SaveReturnAddress(ReturnAddress returnAddress)
        {
            _db.Add(returnAddress);
        }
        public List<LabelDetails> getLabelDetails(string uid)
        {
            var list = _db.LabelDetails.Where(e => e.Uid == uid).OrderByDescending(c=>c.DateCreated).ToList();
            return list;
        }
        public async Task<HttpResponseMessage> CreateUPSLabelAsync(CreateUpsLabel upsOrder, string uid)
        {
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://aio.gg/api/upsv3/order");
            request.Headers.Add("Auth", "c422df81-015d-632d-4a3d-3281c0b4d952");

            if (string.IsNullOrEmpty(upsOrder.order.FromPhone))
            {
                upsOrder.order.FromPhone = GenerateRandomPhoneNumber();
            } if (string.IsNullOrEmpty(upsOrder.order.ToPhone))
            {
                upsOrder.order.ToPhone = GenerateRandomPhoneNumber();
            }

            var collection = new List<KeyValuePair<string, string>>();
            collection.Add(new KeyValuePair<string, string>("FromName", upsOrder.order.FromName));
            collection.Add(new KeyValuePair<string, string>("FromCompany", upsOrder.order.FromCompany));
            collection.Add(new KeyValuePair<string, string>("FromPhone", upsOrder.order.FromPhone)); //if empty, generate random number
            collection.Add(new KeyValuePair<string, string>("FromZip", upsOrder.order.FromZip));
            collection.Add(new KeyValuePair<string, string>("FromAddress", upsOrder.order.FromAddress1));
            collection.Add(new KeyValuePair<string, string>("FromAddress2", upsOrder.order.FromAddress2));
            collection.Add(new KeyValuePair<string, string>("FromCity", upsOrder.order.FromCity));
            collection.Add(new KeyValuePair<string, string>("FromState", upsOrder.order.FromCity));

            collection.Add(new KeyValuePair<string, string>("ToName", upsOrder.order.ToName));
            collection.Add(new KeyValuePair<string, string>("ToCompany", upsOrder.order.ToCompany));
            collection.Add(new KeyValuePair<string, string>("ToPhone", upsOrder.order.ToPhone));
            collection.Add(new KeyValuePair<string, string>("ToZip", upsOrder.order.ToZip));
            collection.Add(new KeyValuePair<string, string>("ToAddress", upsOrder.order.ToAddress1));
            collection.Add(new KeyValuePair<string, string>("ToAddress2", upsOrder.order.ToAddress2));
            collection.Add(new KeyValuePair<string, string>("ToCity", upsOrder.order.ToCity));
            collection.Add(new KeyValuePair<string, string>("ToState", upsOrder.order.ToState));

            collection.Add(new KeyValuePair<string, string>("Weight", upsOrder.order.Weight.ToString()));
            collection.Add(new KeyValuePair<string, string>("Length", upsOrder.order.Length.ToString()));
            collection.Add(new KeyValuePair<string, string>("Width", upsOrder.order.Width.ToString()));
            collection.Add(new KeyValuePair<string, string>("Height", upsOrder.order.Height.ToString()));
            collection.Add(new KeyValuePair<string, string>("Class", upsOrder.serviceClass));

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
                labelDetails.Email = upsOrder.order.Email;
                labelDetails.Uid = uid;
                labelDetails.DateCreated = DateTime.Now.ToString();
                _db.Add(labelDetails);
            }
            _db.SaveChanges();
            if (!response.IsSuccessStatusCode || !createLabelResponse.Success)
            {
                var errorMessage="";
                if (!response.IsSuccessStatusCode)
                {
                   errorMessage  = await response.Content.ReadAsStringAsync();
                }
                if(createLabelResponse.Success == false)
                {
                    
                        errorMessage += createLabelResponse.Error;
                    
                    
                }
                throw new ApplicationException($"Error creating UPS label: {response.StatusCode}. {errorMessage}");
            }

            return response;
        }
        public async Task<FileStreamResult> getLabel(string orderID, bool view = false)
        {
            using var client = new HttpClient();
            var uri = "https://aio.gg/api/upsv3/order/"+orderID+"/file";
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
