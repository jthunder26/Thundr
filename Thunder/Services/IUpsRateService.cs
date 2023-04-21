using Microsoft.AspNetCore.Mvc;
using Thunder.Models;

namespace Thunder.Services
{
    public interface IUpsRateService
    {
        Task<FullRateDTO> GetFullRatesAsync(UpsOrder fullRate);
        Task<QuickRateDTO> GetQuickRatesAsync(NewRate quickRate);
    }
}