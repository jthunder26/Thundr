using Thunder.Data;
using Thunder.Models;
namespace Thunder.Services
{
    public class RetrieveAndStoreLabelBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RetrieveAndStoreLabelBackgroundService> _logger;

        public RetrieveAndStoreLabelBackgroundService(IServiceScopeFactory scopeFactory, ILogger<RetrieveAndStoreLabelBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var blobService = scope.ServiceProvider.GetRequiredService<IBlobService>();
                    var backgroundQueueService = scope.ServiceProvider.GetRequiredService<IBackgroundQueueService>();

                    var workItem = await backgroundQueueService.DequeueRetrieveStore(stoppingToken);

                    if (workItem == null) continue;

                    try
                    {
                        await blobService.SavePdfToBlobAsync(workItem);
                        _logger.LogInformation($"Successfully stored PDF for work item {workItem}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error storing PDF for work item {workItem}");
                        backgroundQueueService.EnqueueRetrieveStore(workItem);  // Requeue for retry
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }


}
