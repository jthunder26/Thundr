using Microsoft.Azure.Storage.Blob.Protocol;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Thunder.Models
{
    public class BankRateDTO
    {
        public int ID { get; set; }
        public string service { get; set; }
    }
    public class BulkRateOrderDetails
    {   
        public string Uid { get; set; }
        public List<UpsOrderDetails> OrderDetails { get; set; }

    }  
    public class BulkRates
    {
        public List<FullRateDTO> Under { get; set; }
        public List<FullRateDTO> Over { get; set; }
        public List<FullRateDTO> Error { get; set; }

    } 
    public class BulkRateDTO
    {
        public List<RateDTO> under { get; set; }
        public List<RateDTO> over { get; set; }
        public List<RateDTO> error { get; set; }
        public RateDTO? selectedUnder { get; set; }
        public RateDTO? selectedOver { get; set; }
        public int underCount { get; set; }
        public int overCount { get; set; }
        public bool isError { get; set; }
        public string? errorMsg { get; set; }
        public int? bulkId { get; set; }

    }
    public class FullRateDTO
    {
        public List<RateDTO>? rates { get; set; }
        public RateDTO? selectedrate { get; set; }
        public bool IsError { get; set; }
        public string? Error { get; set; }
        public int? UpsOrderDetailsId { get; set; }   
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
        public int exactCost { get; set; }
        public string service { get; set; }
        public bool isCheapest { get; set; }
        public bool isFastest { get; set; }
        public bool isBest { get; set; }
        public string deliveryDate { get; set; }
        public string deliveryTime { get; set; }
        public string deliveryDayOfWeek { get; set; }
        public string estimatedDelivery { get; set; }
        public string upsPrice { get; set; }
        public int upsPriceOG { get; set; }
        public string ourPrice { get; set; }
        public string ourPriceString { get; set; }
        public bool isSelected { get; set; }
        public string serviceClass { get; set; }
        public string percentSaved { get; set; }
        public string percentSavedString { get; set; }
        
        public bool usps { get; set; }
        public bool ups { get; set; }
    }
    
    
    public class CreateUpsLabel
    {
        [JsonProperty("rate")]
        public RateDTO Rate { get; set; }
        [JsonProperty("labelId")]
        public int labelId { get; set; }
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
  
    public class NewRate
    {
        public string FromZip { get; set; }
        public string ToZip { get; set; }
        //public string? PackagingType { get; set; }
        public string Length { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string Weight { get; set; }
        public string? Class { get; set; }
    }


}
