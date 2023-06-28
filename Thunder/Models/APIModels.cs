using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Thunder.Models
{
    public class OriginalPrice
    {
        [JsonProperty("From")]
        public int? From { get; set; }
        [JsonProperty("To")]
        public int? To { get; set; }
        [JsonProperty("Price")]
        public int? Price { get; set; }
        [JsonProperty("ID")]
        public string ID { get; set; }
        [JsonProperty("FromFormatted")]
        public string FromFormatted { get; set; }
        [JsonProperty("ToFormatted")]
        public string ToFormatted { get; set; }
        [JsonProperty("PriceFormatted")]
        public string PriceFormatted { get; set; }
    }

    public class TypeObject
    {
        [JsonProperty("ID")]
        public string ID { get; set; }
        [JsonProperty("Handler")]
        public string Handler { get; set; }
        [JsonProperty("ExternalID")]
        public string ExternalID { get; set; }
        [JsonProperty("ExternalClass")]
        public string ExternalClass { get; set; }
        [JsonProperty("Name")]
        public string Name { get; set; }
        [JsonProperty("Courier")]
        public string Courier { get; set; }
        [JsonProperty("International")]
        public bool? International { get; set; }
        [JsonProperty("AltFields")]
        public bool? AltFields { get; set; }
        [JsonProperty("ExtraFields")]
        public bool? ExtraFields { get; set; }
        [JsonProperty("Validate")]
        public bool? Validate { get; set; }
        [JsonProperty("HasDimensions")]
        public bool? HasDimensions { get; set; }
        [JsonProperty("IsCanada")]
        public bool? IsCanada { get; set; }
        [JsonProperty("MaxWeight")]
        public int? MaxWeight { get; set; }
        [JsonProperty("WeightUnit")]
        public string WeightUnit { get; set; }
        [JsonProperty("WeightDecimal")]
        public bool? WeightDecimal { get; set; }
        [JsonProperty("TrackLink")]
        public string TrackLink { get; set; }
        [JsonProperty("Enabled")]
        public bool? Enabled { get; set; }
        [JsonProperty("MaxWeightFormatted")]
        public string MaxWeightFormatted { get; set; }
        [JsonProperty("OriginalPrices")]
        public List<OriginalPrice> OriginalPrices { get; set; }
        [JsonProperty("UserPrices")]
        public int? UserPrices { get; set; }
        [JsonProperty("Prices")]
        public List<OriginalPrice> Prices { get; set; }
    }

    public class ShipsterOrder
    {
        [JsonProperty("ID")]
        public string ID { get; set; }
        [JsonProperty("User")]
        public string User { get; set; }
        [JsonProperty("Type")]
        public string Type { get; set; }
        [JsonProperty("Weight")]
        public int? Weight { get; set; }
        [JsonProperty("Price")]
        public int? Price { get; set; }
        [JsonProperty("Status")]
        public int? Status { get; set; }
        [JsonProperty("ExternalID")]
        public string ExternalID { get; set; }
        [JsonProperty("Tracking")]
        public string Tracking { get; set; }
        [JsonProperty("Notes")]
        public string Notes { get; set; }
        [JsonProperty("Length")]
        public int? Length { get; set; }
        [JsonProperty("Width")]
        public int? Width { get; set; }
        [JsonProperty("Height")]
        public int? Height { get; set; }
        [JsonProperty("Description")]
        public string Description { get; set; }
        [JsonProperty("Reference1")]
        public string Reference1 { get; set; }
        [JsonProperty("Reference2")]
        public string Reference2 { get; set; }
        [JsonProperty("CustomsPrice")]
        public int? CustomsPrice { get; set; }
        [JsonProperty("CustomsDescription")]
        public string CustomsDescription { get; set; }
        [JsonProperty("FromCountry")]
        public string FromCountry { get; set; }
        [JsonProperty("FromName")]
        public string FromName { get; set; }
        [JsonProperty("FromCompany")]
        public string FromCompany { get; set; }
        [JsonProperty("FromPhone")]
        public string FromPhone { get; set; }
        [JsonProperty("FromStreet")]
        public string FromStreet { get; set; }
        [JsonProperty("FromStreet2")]
        public string FromStreet2 { get; set; }
        [JsonProperty("FromCity")]
        public string FromCity { get; set; }
        [JsonProperty("FromState")]
        public string FromState { get; set; }
        [JsonProperty("FromZip")]
        public string FromZip { get; set; }
        [JsonProperty("ToCountry")]
        public string ToCountry { get; set; }
        [JsonProperty("ToName")]
        public string ToName { get; set; }
        [JsonProperty("ToCompany")]
        public string ToCompany { get; set; }
        [JsonProperty("ToPhone")]
        public string ToPhone { get; set; }
        [JsonProperty("ToStreet")]
        public string ToStreet { get; set; }
        [JsonProperty("ToStreet2")]
        public string ToStreet2 { get; set; }
        [JsonProperty("ToCity")]
        public string ToCity { get; set; }
        [JsonProperty("ToState")]
        public string ToState { get; set; }
        [JsonProperty("ToZip")]
        public string ToZip { get; set; }
        [JsonProperty("ShopifyID")]
        public string ShopifyID { get; set; }
        [JsonProperty("ShopifyFulfillment")]
        public string ShopifyFulfillment { get; set; }
        [JsonProperty("Batch")]
        public string Batch { get; set; }
        [JsonProperty("BatchIndex")]
        public int? BatchIndex { get; set; }
        [JsonProperty("Modified")]
        public int? Modified { get; set; }
        [JsonProperty("Added")]
        public int? Added { get; set; }
        [JsonProperty("TypeName")]
        public string TypeName { get; set; }
        [JsonProperty("TypeObject")]
        public TypeObject TypeObject { get; set; }
        [JsonProperty("WeightUnit")]
        public string WeightUnit { get; set; }
        [JsonProperty("Username")]
        public string Username { get; set; }
        [JsonProperty("PriceFormatted")]
        public string PriceFormatted { get; set; }
        [JsonProperty("StatusName")]
        public string StatusName { get; set; }
        [JsonProperty("StatusColor")]
        public string StatusColor { get; set; }
        [JsonProperty("FromFormatted")]
        public string FromFormatted { get; set; }
        [JsonProperty("ToFormatted")]
        public string ToFormatted { get; set; }
        [JsonProperty("Cancellable")]
        public bool? Cancellable { get; set; }
        [JsonProperty("Downloadable")]
        public bool? Downloadable { get; set; }
        [JsonProperty("Duplicatable")]
        public bool? Duplicatable { get; set; }
        [JsonProperty("Refundable")]
        public bool? Refundable { get; set; }
        [JsonProperty("WeightFormatted")]
        public string WeightFormatted { get; set; }
        [JsonProperty("LengthFormatted")]
        public string LengthFormatted { get; set; }
        [JsonProperty("WidthFormatted")]
        public string WidthFormatted { get; set; }
        [JsonProperty("HeightFormatted")]
        public string HeightFormatted { get; set; }
        [JsonProperty("CustomsPriceFormatted")]
        public string CustomsPriceFormatted { get; set; }
        [JsonProperty("TrackLink")]
        public string TrackLink { get; set; }
        [JsonProperty("Trackable")]
        public bool? Trackable { get; set; }
        [JsonProperty("ModifiedFormatted")]
        public string ModifiedFormatted { get; set; }
        [JsonProperty("AddedFormatted")]
        public string AddedFormatted { get; set; }
    }

    public class ShipsterData
    {
        [JsonProperty("Order")]
        public ShipsterOrder Order { get; set; }
    }

    public class ShipsterResponse
    {
        [JsonProperty("Success")]
        public bool Success { get; set; }
        [JsonProperty("Error")]
        public string Error { get; set; }
        [JsonProperty("Data")]
        public ShipsterData Data { get; set; }
    }


    /// <summary>
    /// AIO Below.
    /// </summary>


    public class CustomsItem
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
        public string PriceFormatted { get; set; }
        public int TotalPrice { get; set; }
        public string TotalPriceFormatted { get; set; }
    }
    public class AIOResponse
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public List<ApiError>? Errors { get; set; }
        [JsonProperty("Data")]
        [JsonConverter(typeof(EmptyArrayToObjectConverter<Data>))]
        public Data? Data { get; set; }
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
        public string Class { get; set; }
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
    public class TrackLinkConverter : JsonConverter<string>
    {
        public override string ReadJson(JsonReader reader, Type objectType, string existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.String:
                    return (string)reader.Value;
                case JsonToken.StartObject:
                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonToken.PropertyName && (string)reader.Value == "tracknum")
                        {
                            reader.Read();
                            return (string)reader.Value;
                        }
                    }
                    throw new JsonReaderException("Expected tracknum property");
                default:
                    throw new JsonReaderException($"Unexpected token type: {reader.TokenType}");
            }
        }

        public override void WriteJson(JsonWriter writer, string value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override bool CanRead => true;
    }
}
