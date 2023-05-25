using Newtonsoft.Json;
using Thunder.Data;

namespace Thunder.Models
{
    public class ChargeValidationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? LabelId { get; set; }   
    }
    public class FindUserResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string uid { get; set; }
    }

    public class StripeOptions
    {
        public string ApiKey { get; set; }
        public string WebhookSecret { get; set; }
    }
    public class Item
    {
       [JsonProperty("order")]
        public UpsOrderDetails order { get; set; }
        [JsonProperty("serviceClass")]
        public string? serviceClass { get; set; }
        [JsonProperty("selectedrate")]
        public RateDTO? selectedrate { get; set; }
    }

    public class PaymentIntentCreateRequest
    {
        [JsonProperty("items")]
        public Item[] Items { get; set; }
    }
}
