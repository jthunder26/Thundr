using System.Text;
using Thunder.Data;
using Thunder.Models;

namespace Thunder.Services
{
    public class RetrieveAndStoreLabelBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
       

        public RetrieveAndStoreLabelBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            
        }

        private const int MaxRetries = 5;  // Or whatever maximum retry count you prefer

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var blobService = scope.ServiceProvider.GetRequiredService<IBlobService>();
                    var backgroundQueueService = scope.ServiceProvider.GetRequiredService<IBackgroundQueueService>();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var dequeueResult = await backgroundQueueService.DequeueRetrieveStore(stoppingToken);
                    string base64WorkItem = dequeueResult.Item1;
                    string messageId = dequeueResult.Item2;
                    string popReceipt = dequeueResult.Item3;

                    if (base64WorkItem == null) continue;

                    string workItemString = dequeueResult.Item1;
                    int workItemId;

                    try
                    {
                        byte[] data = Convert.FromBase64String(workItemString);
                        workItemString = Encoding.UTF8.GetString(data);
                        workItemId = int.Parse(workItemString);
                    }
                    catch (FormatException)
                    {
                        // If it fails to convert from Base64, try to parse the original string
                        workItemId = int.Parse(workItemString);
                    }

                    LabelDetail record = dbContext.LabelDetail.FirstOrDefault(x => x.LabelId == workItemId);


                    if (record != null && record.Retries >= MaxRetries)
                    {
                        //_logger.LogError($"Work item {workItem} has exceeded maximum retries");
                        continue;
                    }

                    try
                    {
                        await blobService.SavePdfToBlobAsync(record.OrderId, record.LabelService, record.FromEmail, record.FullName, record.LabelId);
                       // _logger.LogInformation($"Successfully stored PDF for work item {workItem}");
                       //fullname is null here
                        backgroundQueueService.ConfirmRetrieveStore(messageId, popReceipt);
                    }
                    catch (Exception ex)
                    {
                       // _logger.LogError(ex, $"Error storing PDF for work item {workItem}");

                        if (record != null)
                        {
                            record.Retries++;
                            dbContext.SaveChanges();
                        }

                        backgroundQueueService.EnqueueRetrieveStore(workItemString);  // Requeue for retry
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            
            }
        }
    }


}
