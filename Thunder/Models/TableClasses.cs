

using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Thunder.Models
{
    //to make the AIO label
    [Table("UpsOrderDetails")]
    public class UpsOrderDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LabelId { get; set; }
        public int? BulkId { get; set; }
        [JsonProperty("uid")]
        public string Uid { get; set; }

        [JsonProperty("fromEmail")]
        public string? FromEmail { get; set; }

        [JsonProperty("toEmail")]
        public string? ToEmail { get; set; }

        [JsonProperty("fromName")]
        public string? FromName { get; set; }

        [JsonProperty("fromCompany")]
        public string? FromCompany { get; set; }

        [JsonProperty("fromPhone")]
        public string? FromPhone { get; set; }

        [JsonProperty("fromZip")]
        public string FromZip { get; set; }

        [JsonProperty("fromAddress1")]
        public string FromAddress1 { get; set; }

        [JsonProperty("fromAddress2")]
        public string? FromAddress2 { get; set; }

        [JsonProperty("fromCity")]
        public string FromCity { get; set; }

        [JsonProperty("fromState")]
        public string FromState { get; set; }

        [JsonProperty("toName")]
        public string ToName { get; set; }

        [JsonProperty("toCompany")]
        public string? ToCompany { get; set; }

        [JsonProperty("toPhone")]
        public string? ToPhone { get; set; }

        [JsonProperty("toZip")]
        public string ToZip { get; set; }

        [JsonProperty("toAddress1")]
        public string ToAddress1 { get; set; }

        [JsonProperty("toAddress2")]
        public string? ToAddress2 { get; set; }

        [JsonProperty("toCity")]
        public string ToCity { get; set; }

        [JsonProperty("toState")]
        public string ToState { get; set; }

        [JsonProperty("ourPrice")]
        public long? OurPrice { get; set; }

        [JsonProperty("totalAmount")]
        public long? TotalAmount { get; set; }

        [JsonProperty("totalCharge")]
        public long? TotalCharge { get; set; }

        [JsonProperty("ogPrice")]
        public int? OgPrice { get; set; }

        [JsonProperty("percentSaved")]
        public string? PercentSaved { get; set; }


        [JsonProperty("weight")]
        public int Weight { get; set; }

        [JsonProperty("length")]
        public int Length { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("class")]
        public string? Class { get; set; }
        public int checkedOut { get; set; }
        public string? UserName { get; set; }
        public string? Carrier { get; set; }
        public virtual LabelDetail UnfinishedLabel { get; set; }
    }  
    
 
    public class OrderDTO
    {
        [JsonProperty("labelId")]
        public int LabelId { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("orderMessage")]
        public string? OrderMessage { get; set; }

        [JsonProperty("errorMessage")]
        public string? ErrorMessage { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("labelServiceAttempts")]
        public string LabelServiceAttempts { get; set; }

        [JsonProperty("fromEmail")]
        public string? FromEmail { get; set; }

        [JsonProperty("toEmail")]
        public string? ToEmail { get; set; }

        [JsonProperty("fromName")]
        public string? FromName { get; set; }

        [JsonProperty("fromCompany")]
        public string? FromCompany { get; set; }

        [JsonProperty("fromPhone")]
        public string FromPhone { get; set; }

        [JsonProperty("fromZip")]
        public string FromZip { get; set; }

        [JsonProperty("fromAddress1")]
        public string FromAddress1 { get; set; }

        [JsonProperty("fromAddress2")]
        public string? FromAddress2 { get; set; }

        [JsonProperty("fromCity")]
        public string FromCity { get; set; }

        [JsonProperty("fromState")]
        public string FromState { get; set; }

        [JsonProperty("toName")]
        public string ToName { get; set; }

        [JsonProperty("toCompany")]
        public string? ToCompany { get; set; }

        [JsonProperty("toPhone")]
        public string? ToPhone { get; set; }

        [JsonProperty("toZip")]
        public string ToZip { get; set; }

        [JsonProperty("toAddress1")]
        public string ToAddress1 { get; set; }

        [JsonProperty("toAddress2")]
        public string? ToAddress2 { get; set; }

        [JsonProperty("toCity")]
        public string ToCity { get; set; }

        [JsonProperty("toState")]
        public string ToState { get; set; }

        [JsonProperty("weight")]
        public int Weight { get; set; }

        [JsonProperty("length")]
        public int Length { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }


        [JsonProperty("serviceClass")]
        public string? ServiceClass { get; set; }

        [JsonProperty("oGPrice")]
        public string? OGPrice { get; set; }

        [JsonProperty("percentSaved")]
        public string? PercentSaved { get; set; }

        [JsonProperty("ourPrice")]
        public string? OurPrice { get; set; }

        [JsonProperty("totalAmount")]
        public string? TotalAmount { get; set; }

        [JsonProperty("totalCharge")]
        public string? TotalCharge { get; set; }


    }

    public class LabelDetails
    {
        public List<LabelDetail>? FinishedOrders { get; set; }
        public List<LabelDetail>? UnfinishedOrders { get; set; }
    }

    [Table("LabelDetail")]
    public class LabelDetail
    {
        [Key]
        [ForeignKey("UpsOrderDetails")]
        public int LabelId { get; set; }
        public int? BulkId { get; set; }

        public string? Uid { get; set; }

        public string LabelName { get; set; }

        public string? FromEmail { get; set; }

        public int Status { get; set; } //// 0 = not started, 1 = Checkedout, 2 = Payment Success, 3 = Label Creation, 4 = Label Retrieval, 5 = Label Storage, 6 = Completed, 7 = Failed

        public string DateCreated { get; set; }

        public string? Message { get; set; }

        public int Error { get; set; }

        public int Retries { get; set; }

        //New fields
        public string? FullName { get; set; }
        public string? CarrierName { get; set; }
        public string? OrderId { get; set; } //Nullable for now to avoid conflicts with existing data.

        public int LabelService { get; set; } // 1=AIO, 2 = Shipster

        public int AIO_Attempt { get; set; }

        public int Shipster_Attempt { get; set; }

        public string? ErrorMsg { get; set; }
        public string? OgPrice { get; set; }
        public string? PercentSaved { get; set; }
        public string? OurPrice { get; set; }
        public string? TotalCharge { get; set; }

        public virtual UpsOrderDetails UpsOrderDetails { get; set; }
    }


    [Table("RateCosts")]
    public class RateCosts
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RateCostsId { get; set; } // New primary key

        public int LabelId { get; set; } // Now it's a foreign key

        public string serviceClass { get; set; }

        public int TotalCost { get; set; }
        public int? TotalCharge { get; set; }
    }

    [Table("ReturnAddress")]
    public class ReturnAddress
    {
        [Key]
        public string Uid { get; set; }
        public string AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? Company { get; set; }
        public string City { get; set; }
        public string StateProvinceCode { get; set; }
        public string? CountryCode { get; set; }
        public string PostalCode { get; set; }
        public bool IsReturnAddress { get; set; }
    }
}
