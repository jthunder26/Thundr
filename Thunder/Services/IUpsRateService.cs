using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using NuGet.Protocol;
using System.Globalization;
using System.Net.Http.Headers;
using Thunder.Models;
using Stripe;
using System.Transactions;
using System.Runtime.Intrinsics.Arm;

namespace Thunder.Services
{
    public interface IUpsRateService
    {
        Task<FullRateDTO> GetFullRatesAsync(UpsOrderDetails fullRate);
        Task<QuickRateDTO> GetQuickRatesAsync(NewRate quickRate);
    }

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



        private static Dictionary<string, Dictionary<int, double>> DiscountMatrix = new Dictionary<string, Dictionary<int, double>>
        {
            {"ups_ground", new Dictionary<int, double>
                {
                    {1, 0.6080},
                    {5, 0.4876},
                    {10, 0.4069},
                    {15, 0.3377},
                    {20, 0.2846},
                    {25, 0.5865},
                    {30, 0.4847},
                    {35, 0.4696},
                    {40, 0.4477},
                    {45, 0.4555},
                    {50, 0.4463},
                    {55, 0.4485},
                    {60, 0.4452},
                    {65, 0.4473},
                    {70, 0.4413},
                    {75, 0.4265},
                    {80, 0.4254},
                    {85, 0.4159},
                    {90, 0.4169},
                    {95, 0.4247},
                    {100, 0.4239},
                    {105, 0.4135},
                    {110, 0.4151},
                    {115, 0.4129},
                    {120, 0.4091},
                    {125, 0.4110},
                    {130, 0.4174},
                    {135, 0.4131},
                    {140, 0.4114},
                    {145, 0.4167},
                    {150, 0.4134}
                }
            },
            {"ups_3_day_select", new Dictionary<int, double>
                {
                    {1, 0.2808},
                    {5, 0.2506},
                    {10, 0.2304},
                    {15, 0.2407},
                    {20, 0.2325},
                    {25, 0.2140},
                    {30, 0.2086},
                    {35, 0.1982},
                    {40, 0.1976},
                    {45, 0.1977},
                    {50, 0.2099},
                    {55, 0.2635},
                    {60, 0.2647},
                    {65, 0.2612},
                    {70, 0.2610},
                    {75, 0.2520},
                    {80, 0.2510},
                    {85, 0.2658},
                    {90, 0.2915},
                    {95, 0.3007},
                    {100, 0.3083},
                    {105, 0.3075},
                    {110, 0.3149},
                    {115, 0.3164},
                    {120, 0.3210},
                    {125, 0.3176},
                    {130, 0.3200},
                    {135, 0.3193},
                    {140, 0.3187},
                    {145, 0.3214},
                    {150, -0.1000}
                }
            },
            {"ups_2nd_day_air", new Dictionary<int, double>
            {
                {1, 0.1961},
                {5, 0.216},
                {10, 0.242},
                {15, 0.222},
                {20, 0.2275},
                {25, 0.2265},
                {30, 0.2297},
                {35, 0.2232},
                {40, 0.2267},
                {45, 0.221},
                {50, 0.2271},
                {55, 0.2584},
                {60, 0.2656},
                {65, 0.2505},
                {70, 0.2617},
                {75, 0.2482},
                {80, 0.2584},
                {85, 0.2415},
                {90, 0.2464},
                {95, 0.2521},
                {100, 0.2472},
                {105, 0.2455},
                {110, 0.2439},
                {115, 0.2425},
                {120, 0.2412},
                {125, 0.24},
                {130, 0.2388},
                {135, 0.2378},
                {140, 0.2367},
                {145, 0.2358},
                {150, 0.2349}
            }
            },
            {"ups_next_day_air", new Dictionary<int, double>
            {
                {1, 0.2815},
                {5, 0.2783},
                {10, 0.2908},
                {15, 0.2743},
                {20, 0.2721},
                {25, 0.2693},
                {30, 0.2725},
                {35, 0.2597},
                {40, 0.2597},
                {45, 0.2459},
                {50, 0.2548},
                {55, 0.2659},
                {60, 0.2575},
                {65, 0.239},
                {70, 0.2427},
                {75, 0.2477},
                {80, 0.2444},
                {85, 0.2335},
                {90, 0.2398},
                {95, 0.2367},
                {100, 0.2264},
                {105, 0.2257},
                {110, 0.225},
                {115, 0.2244},
                {120, 0.2238},
                {125, 0.2233},
                {130, 0.2228},
                {135, 0.2223},
                {140, 0.2219},
                {145, 0.2215},
                {150, 0.2211}
            }
            },
            {"ups_next_day_air_early", new Dictionary<int, double>
            {
                {1, 0.4797},
                {5, 0.4655},
                {10, 0.4569},
                {15, 0.4252},
                {20, 0.3985},
                {25, 0.3973},
                {30, 0.3914},
                {35, 0.3705},
                {40, 0.3636},
                {45, 0.3432},
                {50, 0.3471},
                {55, 0.3365},
                {60, 0.325},
                {65, 0.3031},
                {70, 0.3046},
                {75, 0.307},
                {80, 0.3011},
                {85, 0.2874},
                {90, 0.2919},
                {95, 0.2866},
                {100, 0.2739},
                {105, 0.2714},
                {110, 0.2691},
                {115, 0.2669},
                {120, 0.2649},
                {125, 0.263},
                {130, 0.2613},
                {135, 0.2596},
                {140, 0.2581},
                {145, 0.2566},
                {150, 0.2553}
            }
             }
        };

       

        private const string BaseUrl = "https://wwwcie.ups.com/ship/v1801/rating/";
        private const string RateEndpoint = "Shoptimeintransit";
        private const string AccessLicenseNumber = "BDCF557031A71B81";
        private const string Username = "ekthunder";
        private const string Password = "Exotics2020!";
        private const string MediaType = "application/json";

        public async Task<FullRateDTO> GetFullRatesAsync(UpsOrderDetails fullRate)
        {
            string errorMsg;
            var upsRequest = CreateUpsRequest(fullRate);
            var httpRequest = BuildHttpRequest(upsRequest);
            var customUpsResponse = await SendHttpRequestAsync(httpRequest);
            if (customUpsResponse.IsError)
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
        private UpsRequest CreateUpsRequest(UpsOrderDetails fullRate)
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
                    customUpsResponse.Response = upsResponse;
                    customUpsResponse.IsError = false;
                }
                return customUpsResponse;
            }


        }
        public class DiscountAndPrice
        {
            public string Price { get; set; }
            public string Discount { get; set; }
        }

        public static DiscountAndPrice ApplyDiscount(string service, double weight, int priceInCents)
        {
            double discountedPriceInCents = priceInCents;
            DiscountAndPrice dp = new DiscountAndPrice();
            if (DiscountMatrix.ContainsKey(service))
            {
                var serviceDiscounts = DiscountMatrix[service];
                int weightKey = -1;

                // Find the appropriate weight key
                foreach (var key in serviceDiscounts.Keys)
                {
                    if (weight <= key)
                    {
                        weightKey = key;
                        break;
                    }
                }

                if (weightKey != -1)
                {
                    double discountFactor = serviceDiscounts[weightKey];
                    discountedPriceInCents = priceInCents * (1 - discountFactor);
                    var discountRounded = Math.Round(discountFactor * 100, 2);
                    dp.Discount = discountRounded.ToString("F2");
                }
            }
            var unroundedPrice = discountedPriceInCents / 100;
            var priceRounded = Math.Round(unroundedPrice, 2);
            dp.Price = priceRounded.ToString("F2");

            // No discount found or applied, return the discounted price or the original price
            return dp;
        }

        public static void SetAttributes(List<RateDTO> rates)
        {
            RateDTO cheapestRate = null;
            RateDTO fastestRate = null;
            RateDTO bestValueRate = null;
            double minPrice = double.MaxValue;
            DateTime minDeliveryDateTime = DateTime.MaxValue;
            double maxPercentSaved = double.MinValue;

            // Find the cheapest rate
            foreach (var rate in rates)
            {
                double price;
                if (double.TryParse(rate.ourPrice, out price))
                {
                    if (price < minPrice)
                    {
                        minPrice = price;
                        cheapestRate = rate;
                    }
                }
            }

            // Find the fastest rate
            foreach (var rate in rates)
            {
                DateTime deliveryDate;
                DateTime deliveryTime;
                if (DateTime.TryParseExact(rate.deliveryDate, "MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out deliveryDate) &&
                    DateTime.TryParseExact(rate.deliveryTime, "hh:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out deliveryTime))
                {
                    DateTime deliveryDateTime = deliveryDate.Add(deliveryTime.TimeOfDay);
                    if (deliveryDateTime < minDeliveryDateTime)
                    {
                        minDeliveryDateTime = deliveryDateTime;
                        fastestRate = rate;
                    }
                }
            }

            // Find the best value rate
            foreach (var rate in rates)
            {
                double percentSaved;
                if (double.TryParse(rate.percentSaved, out percentSaved))
                {
                    if (percentSaved > maxPercentSaved && rate != cheapestRate && rate != fastestRate)
                    {
                        maxPercentSaved = percentSaved;
                        bestValueRate = rate;
                    }
                }
            }

            if (cheapestRate != null)
            {
                cheapestRate.isCheapest = true;
            }

            if (fastestRate != null)
            {
                fastestRate.isFastest = true;
                fastestRate.isSelected = true;
            }

            if (bestValueRate != null)
            {
                bestValueRate.isBestValue = true;
            }
        }





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
                    //if (rate.service == "UPS Next Day Air")
                    //{
                    //    rate.isSelected = true;
                    //}
                    if (dayCode == "SAT")
                    {
                        rate.service += " [Saturday Delivery]";
                    }
                    string day;
                    WeekDayConverter.TryGetValue(dayCode, out day);
                    rate.deliveryDayOfWeek = day;

                    rate.estimatedDelivery = "Estimated Delivery " + rate.deliveryDayOfWeek + " " + rate.deliveryDate + " by " + rate.deliveryTime + " if shipped today";
                    rate.upsPrice = "$" + ratedShipment.TotalCharges.MonetaryValue + " retail";

                    int price = Convert.ToInt32(Convert.ToDouble(ratedShipment.TotalCharges.MonetaryValue) * 100);
                    int weight = Convert.ToInt32(Convert.ToDouble(ratedShipment.BillingWeight.Weight));
                    //rate.ourPrice = (Convert.ToDouble(ratedShipment.TotalCharges.MonetaryValue) * 0.5).ToString("F");
                    //rate.ourPriceString = "$" + rate.ourPrice;
                    var dp = ApplyDiscount(rate.serviceClass, weight, price);
                    rate.ourPrice = dp.Price;
                    rate.percentSaved = dp.Discount;
                    rate.percentSavedString = "Save " + dp.Discount + "%";
                    rate.ourPriceString = "$" + rate.ourPrice;
                    
                   
                        rates.Add(rate);
                   
                }
            }

            //var lowestRate = rates.OrderBy(x => x.ourPrice).FirstOrDefault();
            //lowestRate.isCheapest = true;

            SetAttributes(rates);
            for (int i = rates.Count - 1; i >= 0; i--)
            {
                if (rates[i].isSelected)
                {
                    fullrates.selectedrate = rates[i];
                    rates.RemoveAt(i);
                }
            }
            fullrates.rates = rates;
            return fullrates;
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
                    RateDTO rate = new RateDTO();

                    rate.service = ratedShipment.TimeInTransit.ServiceSummary.Service.Description;
                    var serviceCode = ratedShipment.Service.Code;
                    rate.deliveryDate = ratedShipment.TimeInTransit.ServiceSummary.EstimatedArrival.Arrival.Date[4..6] + "/" + ratedShipment.TimeInTransit.ServiceSummary.EstimatedArrival.Arrival.Date[6..];
                    rate.deliveryTime = DateTime.ParseExact(ratedShipment.TimeInTransit.ServiceSummary.EstimatedArrival.Arrival.Time, "HHmmss", CultureInfo.InvariantCulture).ToString("hh:mm tt");
                    rate.deliveryDayOfWeek = WeekDayConverter[ratedShipment.TimeInTransit.ServiceSummary.EstimatedArrival.DayOfWeek];
                    rate.upsPrice = $"${ratedShipment.TotalCharges.MonetaryValue} retail";
                    string serviceClass;
                    ServiceClass.TryGetValue(serviceCode, out serviceClass);
                    rate.serviceClass = serviceClass;
                    int price = Convert.ToInt32(Convert.ToDouble(ratedShipment.TotalCharges.MonetaryValue) * 100);
                    int weight = Convert.ToInt32(Convert.ToDouble(ratedShipment.BillingWeight.Weight));

                    var dp = ApplyDiscount(rate.serviceClass, weight, price);
                    rate.ourPrice = dp.Price;
                    rate.percentSaved = dp.Discount;
                    rate.percentSavedString = "Save " + dp.Discount + "%";
                    rate.ourPriceString = "$" + rate.ourPrice;
                    rate.estimatedDelivery = $"Estimated Delivery {rate.deliveryDayOfWeek} {rate.deliveryDate} by {rate.deliveryTime} if shipped today";
                    //rate.ourPriceString = $"${rate.ourPrice}";
                    rate.isFastest = false;
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
      

        //FullRate Functions




        //IMPLEMENT THE DISCOUNT


     
        private RateRequest BuildRateRequest(UpsOrderDetails fullRate)
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

        private Shipper BuildShipper(UpsOrderDetails fullRate)
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

        private ShipFrom BuildShipFrom(UpsOrderDetails fullRate)
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

        private ShipTo BuildShipTo(UpsOrderDetails fullRate)
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

        private ShipmentTotalWeight BuildShipmentTotalWeight(UpsOrderDetails fullRate)
        {
            return new ShipmentTotalWeight
            {
                UnitOfMeasurement = new UnitOfMeasurement { Code = "LBS", Description = "Pounds" },
                Weight = fullRate.Weight.ToString(),
            };
        }

        private Package BuildPackage(UpsOrderDetails fullRate)
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