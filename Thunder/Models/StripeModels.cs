using Newtonsoft.Json;

namespace Thunder.Models
{
    public class StripeOptions
    {
        public string ApiKey { get; set; }
        public string WebhookSecret { get; set; }
    }
    public class Item
    {
       [JsonProperty("order")]
        public UpsOrder order { get; set; }
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
