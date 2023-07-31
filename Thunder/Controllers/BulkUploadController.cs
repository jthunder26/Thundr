using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Security.Claims;
using Thunder.Models;
using Thunder.Services;

using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using System.IO;

/////
///The orders should be grouped by weight and then taken the average of the prices within that weight category. 
namespace Thunder.Controllers
{
    [ApiController]
    [Route("FileUpload")]
    public class FileUploadController : ControllerBase
    {
        private readonly IThunderService _thunderService;
        private readonly IUpsRateService _upsRateService;

        public FileUploadController(IThunderService thunderService, IUpsRateService upsRateService)
        {
            _upsRateService = upsRateService;
            _thunderService = thunderService;
        }


        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            BulkRateOrderDetails bulkOrderDetails = new BulkRateOrderDetails();
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);

            bulkOrderDetails.Uid = uid;
            bulkOrderDetails.OrderDetails = ParseFile(file);
            _thunderService.AddBulkOrder(bulkOrderDetails); //add to db, link all by bulkid 

            var bulkRates = await _upsRateService.GetBulkRatesAsync(bulkOrderDetails);

            // Respond with success
            
            
            return Ok(bulkRates);
        }
       


    private List<UpsOrderDetails> ParseFile(IFormFile file)
    {
        var orders = new List<UpsOrderDetails>();

        using (var stream = new MemoryStream())
        {
            file.CopyTo(stream);

            IWorkbook workbook;
            stream.Position = 0;
            workbook = new XSSFWorkbook(stream);

            ISheet sheet = workbook.GetSheetAt(0); // assuming you only have one worksheet

            int rowCount = sheet.LastRowNum;

            for (int row = 1; row <= rowCount; row++)
            {
                IRow rowData = sheet.GetRow(row);

                if (rowData == null || rowData.Cells.All(d => d.CellType == CellType.Blank))
                {
                    continue;
                }

                orders.Add(new UpsOrderDetails
                {
                    ToEmail = rowData.GetCell(0)?.ToString(),
                    ToName = rowData.GetCell(1)?.ToString(),
                    ToCompany = rowData.GetCell(2)?.ToString(),
                    ToAddress1 = rowData.GetCell(3)?.ToString(),
                    ToAddress2 = rowData.GetCell(4)?.ToString(),
                    ToCity = rowData.GetCell(5)?.ToString(),
                    ToState = rowData.GetCell(6)?.ToString(),
                    ToZip = rowData.GetCell(7)?.ToString(),
                    Weight = int.Parse(rowData.GetCell(8)?.ToString()),
                    Length = int.Parse(rowData.GetCell(9)?.ToString()),
                    Width = int.Parse(rowData.GetCell(10)?.ToString()),
                    Height = int.Parse(rowData.GetCell(11)?.ToString()),
                    FromEmail = rowData.GetCell(12)?.ToString(),
                    FromName = rowData.GetCell(13)?.ToString(),
                    FromCompany = rowData.GetCell(14)?.ToString(),
                    FromAddress1 = rowData.GetCell(15)?.ToString(),
                    FromAddress2 = rowData.GetCell(16)?.ToString(),
                    FromCity = rowData.GetCell(17)?.ToString(),
                    FromState = rowData.GetCell(18)?.ToString(),
                    FromZip = rowData.GetCell(19)?.ToString()
                });
            }
        }

        return orders;
    }

}

}