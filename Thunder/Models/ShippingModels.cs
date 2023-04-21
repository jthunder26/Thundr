using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Thunder.Models
{
    public class NewRate
    {
        public string FromZip { get; set; }
        public string ToZip { get; set; }
        //public string? PackagingType { get; set; }
        public string Length { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string Weight { get; set; }

    }
    public class ShipFrom
    {
        public string UserName { get; set; }

        public string Name { get; set; }
        public string Company { get; set; }
        public string Phone { get; set; }
        public string Zip { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }


    }
    public class ShipTo
    {
        public string UserName { get; set; }

        public string Name { get; set; }
        public string Company { get; set; }
        public string Phone { get; set; }
        public string Zip { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }

    }
    public class UserDetails
    {
        public string Email { get; set; }
        public string Phone { get; set; }
        public string FullName { get; set; }
    }
    [Table("ReturnAddress")]
    public class ReturnAddress
    {
        [Key]
        public string Id { get; set; }
        public string AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? Company { get; set; }
        public string City { get; set; }
        public string StateProvinceCode { get; set; }
        public string? CountryCode { get; set; }
        public string PostalCode { get; set; }
        public bool IsReturnAddress { get; set; }
    }

  
    [Table("LabelDetails")]
    public class LabelDetails
    {
        [Key]
        public string Uid { get; set; }
        public string OrderId { get; set; } 
        public string LabelName { get; set; }
        public string Email { get; set; }
        //public string Status { get; set; }
        public string DateCreated { get; set; }
    }
    [Table("UpsOrder")]
    public class UpsOrder
    {
        [Key]
        public string UserName { get; set; }
        public string Email { get; set; }
        public string ToEmail { get; set; } 
        public string FromName { get; set; }
        public string? FromCompany { get; set; }
        public string FromPhone { get; set; }
        public string FromZip { get; set; }
        public string FromAddress1 { get; set; }
        public string? FromAddress2 { get; set; }
        public string FromCity { get; set; }
        public string FromState { get; set; }

        public string ToName { get; set; }
        public string? ToCompany { get; set; }
        public string ToPhone { get; set; }
        public string ToZip { get; set; }
        public string ToAddress1 { get; set; }
        public string? ToAddress2 { get; set; }
        public string ToCity { get; set; }
        public string ToState { get; set; }

        public string Weight { get; set; }
        public int Length { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Class { get; set; }
    }
    public class CreateUpsLabel
    {
        public UpsOrder order { get; set; }
        public string serviceClass { get; set; }    
    }
}
// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class CustomsItem
{
    public string Name { get; set; }
    public int Quantity { get; set; }
    public int Price { get; set; }
    public string PriceFormatted { get; set; }
    public int TotalPrice { get; set; }
    public string TotalPriceFormatted { get; set; }
}

public class Data
{
    public Order Order { get; set; }
}

public class Order
{
    public string ID { get; set; }
    public string User { get; set; }
    public string ExternalID { get; set; }
    public int Provider { get; set; }
    public int Price { get; set; }
    public string FromCountry { get; set; }
    public string FromName { get; set; }
    public string FromStreet { get; set; }
    public string FromStreet2 { get; set; }
    public string FromCity { get; set; }
    public string FromState { get; set; }
    public string FromZip { get; set; }
    public string FromPhone { get; set; }
    public string ToCountry { get; set; }
    public string ToName { get; set; }
    public string ToStreet { get; set; }
    public string ToStreet2 { get; set; }
    public string ToCity { get; set; }
    public string ToState { get; set; }
    public string ToZip { get; set; }
    public string ToPhone { get; set; }
    public string Weight { get; set; }
    public string Length { get; set; }
    public string Width { get; set; }
    public string Height { get; set; }
    public double CustomsPrice { get; set; }
    public string CustomsDescription { get; set; }
    public List<CustomsItem> CustomsItems { get; set; }
    public string Notes { get; set; }
    public bool SignatureRequired { get; set; }
    public bool SaturdayShipping { get; set; }
    public int Class { get; set; }
    public int Status { get; set; }
    public string Track { get; set; }
    public string File { get; set; }
    public int Modified { get; set; }
    public int Added { get; set; }
    public string Username { get; set; }
    public string ProviderName { get; set; }
    public bool IsInternational { get; set; }
    public bool IsManual { get; set; }
    public string PriceFormatted { get; set; }
    public string StatusName { get; set; }
    public string FromFormatted { get; set; }
    public string ToFormatted { get; set; }
    public bool Cancellable { get; set; }
    public bool Downloadable { get; set; }
    public bool Duplicatable { get; set; }
    public bool Refundable { get; set; }
    public bool Trackable { get; set; }
    public bool Processable { get; set; }
    public string SignatureRequiredFormatted { get; set; }
    public string SaturdayShippingFormatted { get; set; }
    public string WeightFormatted { get; set; }
    public string LengthFormatted { get; set; }
    public string WidthFormatted { get; set; }
    public string HeightFormatted { get; set; }
    public string CustomsPriceFormatted { get; set; }
    public string ClassFormatted { get; set; }
    public bool TrackLink { get; set; }
    public string ModifiedFormatted { get; set; }
    public string AddedFormatted { get; set; }
}

public class CreateLabelResponse
{
        public bool Success { get; set; }
        public string Error { get; set; }
        public List<ApiError>? Errors { get; set; }
    [JsonProperty("Data")]
    [JsonConverter(typeof(EmptyArrayToObjectConverter<Data>))]
    public Data? Data { get; set; }
}
    public class ApiError
{
    public string Name { get; set; }
    public string Value { get; set; }
    public string Parameter { get; set; }
    public bool Argument { get; set; }
    public string Error { get; set; }
}

public class EmptyArrayToObjectConverter<T> : JsonConverter<T?> where T : class
{
    public override T? ReadJson(JsonReader reader, Type objectType, T? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.StartArray)
        {
            reader.Skip();
            return null;
        }

        return serializer.Deserialize<T>(reader);
    }

    public override void WriteJson(JsonWriter writer, T? value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}


