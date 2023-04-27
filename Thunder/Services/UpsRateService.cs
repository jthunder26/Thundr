using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;
using Thunder.Models;

namespace Thunder.Services
{
    public class UpsRateService : IUpsRateService
    {
      
        private static readonly Dictionary<string, string> ServiceNames = new Dictionary<string, string>
        {
            {"14", "UPS Next Day Air Early"},
            {"01", "UPS Next Day Air"},
            {"02", "UPS 2nd Day Air"},
            {"12", "UPS 3 Day Select"},
            {"03", "UPS Ground"}
        };  
        private static readonly Dictionary<string, string> ServiceClass = new Dictionary<string, string>
        {
            {"14", "ups_next_day_air_early"},
            {"01", "ups_next_day_air"},
            {"02", "ups_2nd_day_air"},
            {"12", "ups_3_day_select"},
            {"03", "ups_ground"}
        };

        private static readonly Dictionary<string, string> WeekDayConverter = new Dictionary<string, string>
        {
            {"MON", "Monday"},
            {"TUE", "Tuesday"},
            {"WED", "Wednesday"},
            {"THU", "Thursday"},
            {"FRI", "Friday"},
            {"SAT", "Saturday"}
        };

        private const string BaseUrl = "https://wwwcie.ups.com/ship/v1801/rating/";
        private const string RateEndpoint = "Shoptimeintransit";
        private const string AccessLicenseNumber = "BDCF557031A71B81";
        private const string Username = "ekthunder";
        private const string Password = "Exotics2020!";
        private const string MediaType = "application/json";

        public async Task<FullRateDTO> GetFullRatesAsync(UpsOrder fullRate)
        {
            string errorMsg;
            var upsRequest = CreateUpsRequest(fullRate);
            var httpRequest = BuildHttpRequest(upsRequest);
            var customUpsResponse = await SendHttpRequestAsync(httpRequest);
            if(customUpsResponse.IsError)
            {
                errorMsg = customUpsResponse.ErrorMessage;
                FullRateDTO error = new FullRateDTO();
                error.IsError = true;
                error.Error = errorMsg;
                return error;
            }
            var fullRateDto = ProcessFullRateResponse(customUpsResponse.Response);
            return fullRateDto;
        }
        public async Task<QuickRateDTO> GetQuickRatesAsync(NewRate quickRate)
        {
            string errorMsg;
            var upsRequest = CreateUpsRequest(quickRate);
            var httpRequest = BuildHttpRequest(upsRequest);
            var customUpsResponse = await SendHttpRequestAsync(httpRequest);
            if (customUpsResponse.IsError)
            {
                errorMsg = customUpsResponse.ErrorMessage;
                QuickRateDTO error = new QuickRateDTO();
                error.IsError = true;
                error.Error = errorMsg;
                return error;
            }
            var quickRateDTO = ProcessQuickRateResponse(customUpsResponse.Response);
            return quickRateDTO;
        }
        private UpsRequest CreateUpsRequest(UpsOrder fullRate)
        {
            RateRequest rateRequest = BuildRateRequest(fullRate);

            return new UpsRequest { RateRequest = rateRequest };
        }
      
       
        //Shared Functions
       
        private HttpRequestMessage BuildHttpRequest(UpsRequest upsRequest)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, RateEndpoint);
            httpRequest.Headers.Add("transId", "rateRequest");
            httpRequest.Headers.Add("transactionSrc", "00001");
            httpRequest.Headers.Add("AccessLicenseNumber", AccessLicenseNumber);
            httpRequest.Headers.Add("Username", Username);
            httpRequest.Headers.Add("Password", Password);
            httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaType));

            var jsonRequest = upsRequest.ToJson();
            httpRequest.Content = new StringContent(jsonRequest, null, MediaType);

            return httpRequest;
        }
        private async Task<CustomUpsResponse> SendHttpRequestAsync(HttpRequestMessage httpRequest)
        {
            CustomUpsResponse customUpsResponse = new CustomUpsResponse();
            UpsResponse upsResponse;
            using (HttpClient client = new HttpClient { BaseAddress = new Uri(BaseUrl) })
            {
                var response = await client.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    // Reading the content as a string
                    var errorContent = await response.Content.ReadAsStringAsync();

                    // Parsing the JSON content
                    JObject jsonContent = JObject.Parse(errorContent);

                    // Retrieving the error message
                    string errorMessage = jsonContent["response"]["errors"][0]["message"].ToString();

                    // Return the error message
                    customUpsResponse.ErrorMessage = errorMessage;
                    customUpsResponse.IsError = true;
                    return customUpsResponse;
                }
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    upsResponse = JsonConvert.DeserializeObject<UpsResponse>(content);
                    customUpsResponse.Response= upsResponse;
                   customUpsResponse.IsError = false;
                }
                return customUpsResponse;
            }

            
        }



       




        //QuickRate Functions
        private UpsRequest CreateUpsRequest(NewRate quickRate)
        {
            RateRequest rateRequest = BuildQuickRateRequest(quickRate);

            return new UpsRequest { RateRequest = rateRequest };
        }
        private RateRequest BuildQuickRateRequest(NewRate quickRate)
        {
            var rateRequest = new RateRequest
            {
                CustomerClassification = new CustomerClassification { Code = "53" },
                Request = new Request { SubVersion = "2201" },
                Shipment = new Shipment
                {
                    DeliveryTimeInformation = new DeliveryTimeInformation { PackageBillType = "03" },
                    Pickup = new Pickup
                    {
                        Date = DateTime.Now.Date.ToString("yyyyMMdd"),
                        Time = "1900"
                    },
                    Shipper = new Shipper
                    {
                        Address = new Address
                        {
                            PostalCode = quickRate.FromZip,
                            CountryCode = "US"
                        }
                    },
                    ShipFrom = new ShipFrom
                    {
                        Address = new Address
                        {
                            PostalCode = quickRate.FromZip,
                            CountryCode = "US"
                        }
                    },
                    ShipTo = new ShipTo
                    {
                        Address = new Address
                        {
                            PostalCode = quickRate.ToZip,
                            CountryCode = "US"
                        }
                    },
                    ShipmentTotalWeight = new ShipmentTotalWeight
                    {
                        UnitOfMeasurement = new UnitOfMeasurement
                        {
                            Code = "LBS",
                            Description = "Pounds"
                        },
                        Weight = quickRate.Weight
                    },
                    Package = new Package
                    {
                        PackagingType = new PackagingType { Code = "02" },
                        Dimensions = new Dimensions
                        {
                            UnitOfMeasurement = new UnitOfMeasurement { Code = "IN" },
                            Length = quickRate.Length,
                            Width = quickRate.Width,
                            Height = quickRate.Height
                        },
                        PackageWeight = new PackageWeight
                        {
                            UnitOfMeasurement = new UnitOfMeasurement { Code = "LBS" },
                            Weight = quickRate.Weight
                        }
                    }
                }
            };

            return rateRequest;
        }
        private QuickRateDTO ProcessQuickRateResponse(UpsResponse upsResponse)
        {
            QuickRateDTO quickRates = new QuickRateDTO();
            quickRates.IsError = false; 
            var rates = new List<RateDTO>();
            foreach (var ratedShipment in upsResponse.RateResponse.RatedShipment)
            {
                if (ServiceNames.TryGetValue(ratedShipment.Service.Code, out string serviceName))
                {
                    var rate = new RateDTO
                    {
                        service = ratedShipment.TimeInTransit.ServiceSummary.Service.Description,
                        deliveryDate = ratedShipment.TimeInTransit.ServiceSummary.EstimatedArrival.Arrival.Date[4..6] + "/" + ratedShipment.TimeInTransit.ServiceSummary.EstimatedArrival.Arrival.Date[6..],
                        deliveryTime = DateTime.ParseExact(ratedShipment.TimeInTransit.ServiceSummary.EstimatedArrival.Arrival.Time, "HHmmss", CultureInfo.InvariantCulture).ToString("hh:mm tt"),
                        deliveryDayOfWeek = WeekDayConverter[ratedShipment.TimeInTransit.ServiceSummary.EstimatedArrival.DayOfWeek],
                        upsPrice = $"${ratedShipment.TotalCharges.MonetaryValue} retail",
                        ourPrice = (Convert.ToDouble(ratedShipment.TotalCharges.MonetaryValue) * 0.5).ToString("F")
                    };

                    rate.estimatedDelivery = $"Estimated Delivery {rate.deliveryDayOfWeek} {rate.deliveryDate} by {rate.deliveryTime} if shipped today";
                    rate.ourPriceString = $"${rate.ourPrice}";
                    rates.Add(rate);
                }
            }

            var lowestRate = rates.OrderBy(x => x.ourPrice).FirstOrDefault();
            if (lowestRate != null)
            {
                lowestRate.isCheapest = true;
            }

            quickRates.Rates = rates.OrderBy(x => x.ourPrice).ToList();
            return quickRates;
        }


        //FullRate Functions
        private FullRateDTO ProcessFullRateResponse(UpsResponse upsResponse)
        {
            List<RateDTO> rates = new List<RateDTO>();
            FullRateDTO fullrates = new FullRateDTO();
            fullrates.IsError = false;
            var id = 0;
            foreach (var ratedShipment in upsResponse.RateResponse.RatedShipment)
            {
                bool hasService = ServiceNames.ContainsKey(ratedShipment.Service.Code);
                if (hasService)
                {
                   
                    RateDTO rate = new RateDTO();
                    var serviceCode = ratedShipment.Service.Code;
                    rate.ID = id;
                    id++;
                    string unparsedDate = ratedShipment.TimeInTransit.ServiceSummary.EstimatedArrival.Arrival.Date;
                    string parsedDate = unparsedDate.Substring(4, 2) + "/" + unparsedDate.Substring(6, 2);
                    rate.deliveryDate = parsedDate.ToString();

                    var unparsedTime = ratedShipment.TimeInTransit.ServiceSummary.EstimatedArrival.Arrival.Time;
                    DateTime DT = DateTime.ParseExact(unparsedTime, "HHmmss", new System.Globalization.CultureInfo("en-US"));
                    var n = DT.ToString("hh:mm tt");
                    rate.deliveryTime = n;

                        string serviceClass;
                        ServiceClass.TryGetValue(serviceCode, out serviceClass);
                        rate.serviceClass = serviceClass;
                        rate.service = ratedShipment.TimeInTransit.ServiceSummary.Service.Description;
                    
                            var dayCode = ratedShipment.TimeInTransit.ServiceSummary.EstimatedArrival.DayOfWeek;
                            if (rate.service == "UPS Next Day Air")
                            {
                                rate.isSelected = true;
                            }
                            if (dayCode == "SAT")
                            {
                                rate.service += " [Saturday Delivery]";
                            }
                            string day;
                            WeekDayConverter.TryGetValue(dayCode, out day);
                            rate.deliveryDayOfWeek = day;
                    
                    rate.estimatedDelivery = "Estimated Delivery " + rate.deliveryDayOfWeek + " " + rate.deliveryDate + " by " + rate.deliveryTime + " if shipped today";
                    rate.upsPrice = "$" + ratedShipment.TotalCharges.MonetaryValue + " retail";

                    rate.ourPrice = (Convert.ToDouble(ratedShipment.TotalCharges.MonetaryValue) * 0.5).ToString("F");
                    rate.ourPriceString = "$" + rate.ourPrice;

                    if (rate.isSelected)
                    {
                        fullrates.selectedrate = rate;
                    }
                    else
                    {
                        rates.Add(rate);
                    }
                }
            }

                var lowestRate = rates.OrderBy(x => x.ourPrice).FirstOrDefault();
                lowestRate.isCheapest = true;
                fullrates.rates = rates.OrderBy(x => x.ourPrice).ToList();
            
            return fullrates;
        }
        private RateRequest BuildRateRequest(UpsOrder fullRate)
        {
            return new RateRequest
            {
                CustomerClassification = new CustomerClassification { Code = "53" },
                Request = new Request { SubVersion = "2201" },
                Shipment = new Shipment
                {
                    DeliveryTimeInformation = new DeliveryTimeInformation { PackageBillType = "03" },
                    Pickup = new Pickup { Date = DateTime.Now.Date.ToString("yyyyMMdd"), Time = "1900" },
                    Shipper = BuildShipper(fullRate),
                    ShipFrom = BuildShipFrom(fullRate),
                    ShipTo = BuildShipTo(fullRate),
                    ShipmentTotalWeight = BuildShipmentTotalWeight(fullRate),
                    Package = BuildPackage(fullRate),
                },
            };
        }
       
        private Shipper BuildShipper(UpsOrder fullRate)
        {
            return new Shipper
            {
                Address = new Address
                {
                    AddressLine = new[] { fullRate.FromAddress1, fullRate.FromAddress2 ?? "" },
                    City = fullRate.FromCity,
                    PostalCode = fullRate.FromZip,
                    StateProvinceCode = fullRate.FromState,
                    CountryCode = "US",
                },
            };
        }

        private ShipFrom BuildShipFrom(UpsOrder fullRate)
        {
            return new ShipFrom
            {
                Address = new Address
                {
                    AddressLine = new[] { fullRate.FromAddress1, fullRate.FromAddress2 ?? "" },
                    City = fullRate.FromCity,
                    PostalCode = fullRate.FromZip,
                    StateProvinceCode = fullRate.FromState,
                    CountryCode = "US",
                },
            };
        }

        private ShipTo BuildShipTo(UpsOrder fullRate)
        {
            return new ShipTo
            {
                Address = new Address
                {
                    AddressLine = new[] { fullRate.ToAddress1, fullRate.ToAddress2 ?? "" },
                    City = fullRate.ToCity,
                    PostalCode = fullRate.ToZip,
                    StateProvinceCode = fullRate.ToState,
                    CountryCode = "US",
                },
            };
        }

        private ShipmentTotalWeight BuildShipmentTotalWeight(UpsOrder fullRate)
        {
            return new ShipmentTotalWeight
            {
                UnitOfMeasurement = new UnitOfMeasurement { Code = "LBS", Description = "Pounds" },
                Weight = fullRate.Weight,
            };
        }

        private Package BuildPackage(UpsOrder fullRate)
        {
            return new Package
            {
                PackagingType = new PackagingType { Code = "02" },
                Dimensions = new Dimensions
                {
                    UnitOfMeasurement = new UnitOfMeasurement { Code = "IN" },
                    Length = fullRate.Length.ToString(),
                    Width = fullRate.Width.ToString(),
                    Height = fullRate.Height.ToString(),
                },
                PackageWeight = new PackageWeight
                {
                    UnitOfMeasurement = new UnitOfMeasurement { Code = "LBS" },
                    Weight = fullRate.Weight.ToString(),
                },
            };
        }

    }
    public class RatedShipmentAlertConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<RatedShipmentAlert>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                return serializer.Deserialize<List<RatedShipmentAlert>>(reader);
            }
            else
            {
                RatedShipmentAlert singleAlert = serializer.Deserialize<RatedShipmentAlert>(reader);
                return new List<RatedShipmentAlert> { singleAlert };
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

}