using Microsoft.AspNetCore.Mvc;
using Thunder.Models;

namespace Thunder.Services
{
    public interface IThunderService
    {
        void SaveReturnAddress(ReturnAddress returnAddress);
        Task<HttpResponseMessage> CreateUPSLabelAsync(CreateUpsLabel upsOrder, string uid);
        UserDetails GetUserDeets(string uid);
        ReturnAddress GetUserAddress(string uid);
        List<LabelDetails> getLabelDetails(string uid);
        Task<FileStreamResult> getLabel(string orderID, bool view = false);
        void UpdateAddress(string uid, ReturnAddress address);
    }
}
