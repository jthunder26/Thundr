//// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using Thunder.Services;

public class UpsRequest
{
    public RateRequest RateRequest { get; set; }
}
public class RateRequest
{
    public Request Request { get; set; }
    public Shipment Shipment { get; set; }
    public CustomerClassification CustomerClassification { get; set; }
}

public class CustomerClassification
{
    public string Code { get; set; }
}

public class Request
{
    public string SubVersion { get; set; }
    public TransactionReference? TransactionReference { get; set; }
}
public class TransactionReference
{
    public string CustomerContext { get; set; }
    public string? TransactionIdentifier { get; set; }
}

public class Shipment
{
    public DeliveryTimeInformation DeliveryTimeInformation { get; set; }
    public Pickup Pickup { get; set; }
    public Shipper Shipper { get; set; }
    public ShipTo ShipTo { get; set; }
    public ShipFrom ShipFrom { get; set; }
    public Service? Service { get; set; }
    public ShipmentTotalWeight ShipmentTotalWeight { get; set; }
    public Package Package { get; set; }
    //public ShipmentRatingOptions ShipmentRatingOptions { get; set; }
}
public class Shipper
{
    public string? Name { get; set; }
    //public string? ShipperNumber { get; set; }
    public Address Address { get; set; }
}

public class Address
{
    public string FullName { get; set; }
    public string[] AddressLine { get; set; }
    public string? Company { get; set; }
    public string City { get; set; }
    public string StateProvinceCode { get; set; }
    public string? CountryCode { get; set; }
    public string PostalCode { get; set; }
}

public class DeliveryTimeInformation
{
    public string PackageBillType { get; set; }
}
//public class Pickup
//{
//    public string Date { get; set; }
//    public string Time { get; set; }
//}

public class Package
{
    public PackagingType PackagingType { get; set; }
    public Dimensions Dimensions { get; set; }
    public PackageWeight PackageWeight { get; set; }
}
public class Dimensions
{
    public UnitOfMeasurement UnitOfMeasurement { get; set; }
    public string Length { get; set; }
    public string Width { get; set; }
    public string Height { get; set; }
}
public class PackageWeight
{
    public UnitOfMeasurement UnitOfMeasurement { get; set; }
    public string Weight { get; set; }
}

public class PackagingType
{
    public string Code { get; set; }
    public string? Description { get; set; }
}




public class Root
{
    public RateRequest RateRequest { get; set; }
}

public class Service
{
    public string Code { get; set; }
    public string Description { get; set; }
}

public class ShipFrom
{
    public string? Name { get; set; }
    public Address Address { get; set; }
}


//dont need
//public class ShipmentRatingOptions
//{
//    public string UserLevelDiscountIndicator { get; set; }
//}

public class ShipmentTotalWeight
{
    public UnitOfMeasurement UnitOfMeasurement { get; set; }
    public string Weight { get; set; }
}

public class UnitOfMeasurement
{
    public string Code { get; set; }
    public string Description { get; set; }
}

public class ShipTo
{
    public string? Name { get; set; }
    public Address Address { get; set; }
}

public class BankRateDTO
{
    public int ID { get; set; }
    public string service { get; set; }
}

public class FullRateDTO
{
    public List<RateDTO>? rates { get; set; }
    public RateDTO? selectedrate { get; set; }
    public bool IsError { get; set; }   
    public string? Error { get; set; }
}
public class QuickRateDTO
{
    public List<RateDTO>? Rates { get; set; }
    public bool IsError { get; set; }
    public string? Error { get; set; }
}
public class RateDTO
{
    public int ID { get; set; }
    public string service { get; set; }
    public bool isCheapest { get; set; }
    public bool isFastest { get; set; }
    public bool isBestValue { get; set; }
    public string deliveryDate { get; set; }
    public string deliveryTime { get; set; }
    public string deliveryDayOfWeek { get; set; }
    public string estimatedDelivery { get; set; }
    public string upsPrice { get; set; }
    public string ourPrice { get; set; }
    public string ourPriceString { get; set; }  
    public bool isSelected { get; set; }
    public string serviceClass { get; set; }
    public string percentSaved { get; set; }
    public string percentSavedString { get; set; }
}

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class Alert
{
    public string Code { get; set; }
    public string Description { get; set; }
}

public class Arrival
{
    public string Date { get; set; }
    public string Time { get; set; }
}

public class BaseServiceCharge
{
    public string CurrencyCode { get; set; }
    public string MonetaryValue { get; set; }
}

public class BillingWeight
{
    public UnitOfMeasurement UnitOfMeasurement { get; set; }
    public string Weight { get; set; }
}

public class EstimatedArrival
{
    public Arrival Arrival { get; set; }
    public string BusinessDaysInTransit { get; set; }
    public Pickup Pickup { get; set; }
    public string DayOfWeek { get; set; }
    public string CustomerCenterCutoff { get; set; }
}

public class GuaranteedDelivery
{
    public string BusinessDaysInTransit { get; set; }
    public string DeliveryByTime { get; set; }
}

public class ItemizedCharges
{
    [JsonProperty("Code")]
    public string Code { get; set; }

    [JsonProperty("CurrencyCode")]
    public string CurrencyCode { get; set; }

    [JsonProperty("MonetaryValue")]
    public string MonetaryValue { get; set; }

    [JsonProperty("SubType", NullValueHandling = NullValueHandling.Ignore)]
    public string SubType { get; set; }
}


    public class Pickup
{
    public string Date { get; set; }
    public string Time { get; set; }
}

public class RatedPackage
{
    public TransportationCharges TransportationCharges { get; set; }
    public BaseServiceCharge BaseServiceCharge { get; set; }
    public ServiceOptionsCharges ServiceOptionsCharges { get; set; }
    [JsonProperty("ItemizedCharges")]
    [JsonConverter(typeof(ItemizedChargesConverter))]
    public ItemizedCharges[] ItemizedCharges { get; set; }
    public TotalCharges TotalCharges { get; set; }
    public string Weight { get; set; }
    public BillingWeight BillingWeight { get; set; }
}

public class RatedShipment
{
    public Service Service { get; set; }
    [JsonConverter(typeof(RatedShipmentAlertConverter))]
    public List<RatedShipmentAlert> RatedShipmentAlert { get; set; }
    public BillingWeight BillingWeight { get; set; }
    public TransportationCharges TransportationCharges { get; set; }
    public BaseServiceCharge BaseServiceCharge { get; set; }
    [JsonProperty("ItemizedCharges")]
    [JsonConverter(typeof(ItemizedChargesConverter))]
    public ItemizedCharges[] ItemizedCharges { get; set; }
    public ServiceOptionsCharges ServiceOptionsCharges { get; set; }
    public TotalCharges TotalCharges { get; set; }
    public GuaranteedDelivery GuaranteedDelivery { get; set; }
    public RatedPackage RatedPackage { get; set; }
    public TimeInTransit TimeInTransit { get; set; }
}

public class RatedShipmentAlert
{
    public string Code { get; set; }
    public string Description { get; set; }
}

public class RateResponse
{
    public Response Response { get; set; }
    public List<RatedShipment> RatedShipment { get; set; }
}

public class Response
{
    public ResponseStatus ResponseStatus { get; set; }
    public List<Alert> Alert { get; set; }
    public TransactionReference TransactionReference { get; set; }
}

public class ResponseStatus
{
    public string Code { get; set; }
    public string Description { get; set; }
}
public class CustomUpsResponse
{
    public bool IsError { get; set; }
    public string ErrorMessage { get; set; }
    public UpsResponse Response { get; set; }
}
public class UpsResponse
{
    public RateResponse RateResponse { get; set; }
}

//public class Service
//{
//    public string Code { get; set; }
//    public string Description { get; set; }
//}

public class ServiceOptionsCharges
{
    public string CurrencyCode { get; set; }
    public string MonetaryValue { get; set; }
}

public class ServiceSummary
{
    public Service Service { get; set; }
    public EstimatedArrival EstimatedArrival { get; set; }
    public string SaturdayDelivery { get; set; }
    public string SaturdayDeliveryDisclaimer { get; set; }
    public string GuaranteedIndicator { get; set; }
}

public class TimeInTransit
{
    public string PickupDate { get; set; }
    public string PackageBillType { get; set; }
    public string Disclaimer { get; set; }
    public ServiceSummary ServiceSummary { get; set; }
}

public class TotalCharges
{
    public string CurrencyCode { get; set; }
    public string MonetaryValue { get; set; }
}

//public class TransactionReference
//{
//    public string CustomerContext { get; set; }
//    public string TransactionIdentifier { get; set; }
//}

public class TransportationCharges
{
    public string CurrencyCode { get; set; }
    public string MonetaryValue { get; set; }
}

//public class UnitOfMeasurement
//{
//    public string Code { get; set; }
//    public string Description { get; set; }
//}

