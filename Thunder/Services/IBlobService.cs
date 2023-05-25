using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using System.IO;
namespace Thunder.Services
{
    public interface IBlobService
    {
        Task<string> SavePdfToBlobAsync(string orderID);
        Task<Stream> GetPdfFromBlobAsync(string orderID);
    }
    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public BlobService(BlobServiceClient blobServiceClient, string containerName)
        {
            _blobServiceClient = blobServiceClient;
            _containerName = containerName;
        }

        public async Task<string> SavePdfToBlobAsync(string orderID)
        {
            using var client = new HttpClient();
            var uri = "https://aio.gg/api/upsv3/order/" + orderID + "/file";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Auth", "c422df81-015d-632d-4a3d-3281c0b4d952");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            // Read the PDF content into a byte array
            var pdfBytes = await response.Content.ReadAsByteArrayAsync();

            // Store the PDF in Azure Blob Storage
            var blobName = $"{orderID}.pdf"; // Use a unique blob name based on the order ID
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            await using (var stream = new MemoryStream(pdfBytes))
            {
                await blobClient.UploadAsync(stream, true);
            }

            return blobClient.Uri.ToString();
        }

        public async Task<Stream> GetPdfFromBlobAsync(string orderID)
        {
            var blobName = $"{orderID}.pdf"; // Use the same blob name based on the order ID
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var response = await blobClient.DownloadAsync();

            return response.Value.Content;
        }
    }
}
