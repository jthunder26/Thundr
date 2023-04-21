using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
//using RateWSSample.RateWebReference;
// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Thunder.Controllers
{
    //NOTE: For security ima store Username and Password in User-Secrets, uncomment the next 2 lines
    //private readonly string upsUsername = WebConfigurationManager.AppSettings["Username"];
    //private readonly string upsPW = WebConfigurationManager.AppSettings["Password"];
    //dotnet user-secrets set "UPSSecurityUserNameToken:Username" "myUserName"
    //dotnet user-secrets set "UPSSecurityUserNameToken:Password" "12345"

    public class UPSController : Controller
    {
        // public void WeatherDetail(string City)
        //{

        ////Assign API KEY which received from OPENWEATHERMAP.ORG  
        //string appId = "d40b1d570cd62f96576a2e6e2850edf3";

        ////API path with CITY parameter and other parameters.  
        //string url = string.Format("http://api.openweathermap.org/data/2.5/weather?q={0}&units=metric&cnt=1&APPID={1}", City, appId);

        //using (WebClient client = new WebClient())
        //{
        //    string json = client.DownloadString(url);

        //    //********************//  
        //    //     JSON RECIVED   
        //    //********************//  
        //    //{"coord":{ "lon":72.85,"lat":19.01},  
        //    //"weather":[{"id":711,"main":"Smoke","description":"smoke","icon":"50d"}],  
        //    //"base":"stations",  
        //    //"main":{"temp":31.75,"feels_like":31.51,"temp_min":31,"temp_max":32.22,"pressure":1014,"humidity":43},  
        //    //"visibility":2500,  
        //    //"wind":{"speed":4.1,"deg":140},  
        //    //"clouds":{"all":0},  
        //    //"dt":1578730750,  
        //    //"sys":{"":1,"id":9052,"country":"IN","sunrise":1578707041,"sunset":1578746875},  
        //    //"timezone":19800,  
        //    //"id":1275339,  
        //    //"name":"Mumbai",  
        //    //"cod":200}  

        //    //Converting to OBJECT from JSON string.  
        //    RootObject weatherInfo = (new JavaScriptSerializer()).Deserialize<RootObject>(json);

        //    //Special VIEWMODEL design to send only required fields not all fields which received from   
        //    //www.openweathermap.org api  
        //    ResultViewModel rslt = new ResultViewModel();

        //    rslt.Country = weatherInfo.sys.country;
        //    rslt.City = weatherInfo.name;
        //    rslt.Lat = Convert.ToString(weatherInfo.coord.lat);
        //    rslt.Lon = Convert.ToString(weatherInfo.coord.lon);
        //    rslt.Description = weatherInfo.weather[0].description;
        //    rslt.Humidity = Convert.ToString(weatherInfo.main.humidity);
        //    rslt.Temp = Convert.ToString(weatherInfo.main.temp);
        //    rslt.TempFeelsLike = Convert.ToString(weatherInfo.main.feels_like);
        //    rslt.TempMax = Convert.ToString(weatherInfo.main.temp_max);
        //    rslt.TempMin = Convert.ToString(weatherInfo.main.temp_min);
        //    rslt.WeatherIcon = weatherInfo.weather[0].icon;

        //    //Converting OBJECT to JSON String   
        //    var jsonstring = new JavaScriptSerializer().Serialize(rslt);

        //    //Return JSON string.  
        //    return jsonstring;
        //  }
        // }
        public async Task UPSRateAsync()
        {
            var client = new HttpClient();
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://wwwcie.ups.com/ship/v1801/rating/Shop?timeintransit");
            httpRequest.Headers.Add("transId", "AAA1");
            httpRequest.Headers.Add("transactionSrc", "Me1");
            httpRequest.Headers.Add("AccessLicenseNumber", "BDCF557031A71B81");
            httpRequest.Headers.Add("Username", "ekthunder");
            httpRequest.Headers.Add("Password", "Exotics2020!");

            RateRequest rateRequest = new RateRequest();

            CustomerClassification customerClassification = new CustomerClassification();
            customerClassification.Code = "53"; //53 is standard rates
            rateRequest.CustomerClassification = customerClassification;

            Request request = new Request();
            request.SubVersion = "2201";
            //request.TransactionReference.CustomerContext = "";
            rateRequest.Request = request;

            Shipment shipment = new Shipment();


            DeliveryTimeInformation deliveryTimeInformation = new DeliveryTimeInformation();
            deliveryTimeInformation.PackageBillType = "03"; //03 is packages
            shipment.DeliveryTimeInformation = deliveryTimeInformation;

            Pickup pickup = new Pickup();
            pickup.Date = DateTime.Now.Date.ToString("yyyyMMdd");
            pickup.Time = "1900"; //7pm
            shipment.Pickup = pickup;

            Shipper shipper = new Shipper();


            Address shipperAddress = new Address();

            //String[] addressLine = { "5555 main", "4 Case Cour", "Apt 3B" }; // We really do not need an acutal address line for rates
            //shipperAddress.AddressLine = addressLine; 
            shipperAddress.City = "Roswell";
            shipperAddress.PostalCode = "30076";
            shipperAddress.StateProvinceCode = "GA";
            shipperAddress.CountryCode = "US";
            //shipperAddress.AddressLine = addressLine;
            shipper.Address = shipperAddress;
            shipment.Shipper = shipper;

            ShipFrom shipFrom = new ShipFrom();
            Address shipFromAddress = new Address();
            //shipFromAddress.AddressLine = addressLine; 
            shipFromAddress.City = "Roswell";
            shipFromAddress.PostalCode = "30076";
            shipFromAddress.StateProvinceCode = "GA";       //Find these values in the API documentation
            shipFromAddress.CountryCode = "US";     //Find these values in the API documentation
            shipFrom.Address = shipFromAddress;
            shipment.ShipFrom = shipFrom;

            ShipTo shipTo = new ShipTo();
            Address shipToAddress = new Address();
            //String[] addressLine1 = { "10 E. Ritchie Way", "2", "Apt 3B" }; //same as above
            //shipToAddress.AddressLine = addressLine1;
            shipToAddress.City = "Plam Springs";
            shipToAddress.PostalCode = "92262";
            shipToAddress.StateProvinceCode = "CA";     //Find these values in the API documentation
            shipToAddress.CountryCode = "US";           //Find these values in the API documentation
            shipTo.Address = shipToAddress;
            shipment.ShipTo = shipTo;

            //Service.Code - not required due to RequestOptions = Shop and not Rate. Shop returns all services
            //Service service = new Service();
            //service.Code = "02";
            //shipment.Service = service;

            ShipmentTotalWeight shipmentTotalWeight = new ShipmentTotalWeight();


            UnitOfMeasurement uom = new UnitOfMeasurement();
            uom.Code = "LBS";
            uom.Description = "Pounds";
            shipmentTotalWeight.UnitOfMeasurement = uom;
            shipmentTotalWeight.Weight = "10";
            shipment.ShipmentTotalWeight = shipmentTotalWeight;

            Package package = new Package();
            PackagingType packagingType = new PackagingType();
            packagingType.Code = "02";
            package.PackagingType = packagingType;

            Dimensions dimensions = new Dimensions();
            UnitOfMeasurement uom2 = new UnitOfMeasurement();
            uom2.Code = "IN"; //IN or CM are valid. Inches or Centimeters
            dimensions.UnitOfMeasurement = uom2;
            dimensions.Length = "10";
            dimensions.Width = "10";
            dimensions.Height = "10";
            package.Dimensions = dimensions;

            PackageWeight packageWeight = new PackageWeight();
            UnitOfMeasurement uom3 = new UnitOfMeasurement();
            uom3.Code = "LBS";
            packageWeight.UnitOfMeasurement = uom3;
            packageWeight.Weight = "7";
            package.PackageWeight = packageWeight;

            //Package[] pkgArray = { package };

            shipment.Package = package;
            rateRequest.Shipment = shipment;

            var jsonRequest = rateRequest.ToJson();
            httpRequest.Content = new StringContent(jsonRequest);
            var response = await client.SendAsync(httpRequest);
            //response.EnsureSuccessStatusCode();
            Console.WriteLine(await response.Content.ReadAsStringAsync());

        }
        // GET: /<controller>/
        //    public IActionResult GetUPSRates()
        //    {



        //        RateRequest rateRequest = new RateRequest();

        //        Request request = new Request();
        //        rateRequest.Request = request;

        //        Shipment shipment = new Shipment();
        //        Shipper shipper = new Shipper();
        //        var shipperAddress = new Address();

        //        //shipper.ShipperNumber = "Your Shipper Number"; //Optional in some cases
        //        String[] addressLine = { "5555 main", "4 Case Cour", "Apt 3B" }; //Apparently this is optional too
        //        shipperAddress.AddressLine = addressLine; // So, test rate here by removing the AddressLine Property
        //        shipperAddress.City = "Roswell";
        //        shipperAddress.PostalCode = "30076";
        //        shipperAddress.StateProvinceCode = "GA";
        //        shipperAddress.CountryCode = "US";
        //        shipperAddress.AddressLine = addressLine;
        //        shipper.Address = shipperAddress;
        //        shipment.Shipper = shipper;

        //        var shipFrom = new ShipFrom();
        //        var shipFromAddress = new ShipAddress();
        //        shipFromAddress.AddressLine = addressLine;      //same as above, idk how its not needed
        //        shipFromAddress.City = "Roswell";
        //        shipFromAddress.PostalCode = "30076";
        //        shipFromAddress.StateProvinceCode = "GA";       //Find these values in the API documentation
        //        shipFromAddress.CountryCode = "US";     //Find these values in the API documentation
        //        shipFrom.Address = shipFromAddress;
        //        shipment.ShipFrom = shipFrom;

        //        ShipTo shipTo = new ShipTo();
        //        ShipToAddress shipToAddress = new ShipToAddress();
        //        String[] addressLine1 = 
        //            { "10 E. Ritchie Way", "2", "Apt 3B" }; //same as above
        //        shipToAddress.AddressLine = addressLine1;
        //        shipToAddress.City = "Plam Springs";
        //        shipToAddress.PostalCode = "92262";
        //        shipToAddress.StateProvinceCode = "CA";     //Find these values in the API documentation
        //        shipToAddress.CountryCode = "US";           //Find these values in the API documentation
        //        shipTo.Address = shipToAddress;
        //        shipment.ShipTo = shipTo;

        //        CodeDescription service = new CodeDescription();
        //        //Below code uses dummy date for reference, I think Entity Frameworks will hitttttt 
        //        service.Code = "02";
        //        shipment.Service = service;

        //        Package package = new Package();
        //        PackageWeight packageWeight = new PackageWeight();
        //        packageWeight.Weight = "125";
        //        CodeDescription uom = new CodeDescription();
        //        uom.Code = "LBS";
        //        uom.Description = "pounds";
        //        packageWeight.UnitOfMeasurement = uom;
        //        package.PackageWeight = packageWeight;
        //        CodeDescription pack = new CodeDescription();
        //        pack.Code = "02";
        //        package.Packaging = pack;
        //        Package[] pkgArray = { package };

        //        shipment.Package = pkgArray;
        //        rateRequest.Shipment = shipment;

        //        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocol.Tls12 | System.Net.SecurityProtocol.Tls | System.Net.SecurityProtocol.Tls11; //This line will ensure the latest security protocol for consuming the web service call.
        //        Console.WriteLine(rateRequest); //remove the Console.WriteLines once testing is comeplete

        //        //var client = new RatePortClient();//either use RatePortClient for ProcessRate or
        //                                            //fix the Assembly Reference error for SoapHttpClientProtocol below

        //        RateResponse rateResponse = rate.ProcessRate(rateRequest);

        //        //Console.WriteLine("The transaction was a " + rateResponse.Response.ResponseStatus.Description);
        //       // Console.WriteLine("Total Shipment Charges " + rateResponse.RatedShipment[0].TotalCharges.MonetaryValue + rateResponse.RatedShipment[0].TotalCharges.CurrencyCode);
        //        //Console.ReadKey();




        //        //Next step: Run Tests API call once errors are cleared, check rateResponse var for JSON response
        //        //Then, Create a RatePackage class with models for the response we are going to receive from UPS
        //        // Thunder.Models => public class RatePackage { public RateResponse response {get;set;}
        //        //Then, back here: var model = new RatePackage(){ response = rateResponse};
        //        //Finally, send that jawn back to the view => return View(model)
        //        //check ur notes on what will be best for front-end visualization; Entity Frameworks?? Angular?? regular JS??
        //        return View();
        //    }
    }

}



