using Thunder.Data;
using Thunder.Models;
namespace Thunder.Services
{
    public class CreateLabelBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CreateLabelBackgroundService> _logger;

        public CreateLabelBackgroundService(IServiceScopeFactory scopeFactory, ILogger<CreateLabelBackgroundService> logger)
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
                    var thunderService = scope.ServiceProvider.GetRequiredService<IThunderService>();
                    var backgroundQueueService = scope.ServiceProvider.GetRequiredService<IBackgroundQueueService>();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var workItem = await backgroundQueueService.DequeueCreateLabel(stoppingToken);

                    if (workItem == null) continue;

                    UnfinishedLabel record = dbContext.UnfinishedLabel.FirstOrDefault(x => x.LabelId == int.Parse(workItem));

                    try
                    {
                        await thunderService.CreateAIOLabelAsync(int.Parse(workItem));
                        _logger.LogInformation($"Successfully processed work item {workItem}");
                        record.Status = 3;
                        record.Message = "Successfully processed AIO Label with Id: " + workItem;

                        backgroundQueueService.EnqueueRetrieveStore(workItem);
                    }
                    catch (ApplicationException ex)
                    {
                        _logger.LogError(ex, $"Error processing AIO work item {workItem}");
                        record.Status = 0;
                        record.Error = 1;
                        record.Message = "Error processing AIO for Label Id:" + workItem + ". " + ex.Message;

                        try
                        {
                            await thunderService.CreateShipsterLabelAsync(int.Parse(workItem));
                            _logger.LogInformation($"Successfully processed work item {workItem} with CreateShipsterLabelAsync");
                            record.Status = 3;
                            record.Error = 0;
                            record.Message += " | Successfully processed Shipster Label with Id: " + workItem;

                            backgroundQueueService.EnqueueRetrieveStore(workItem);
                        }
                        catch (Exception shipsterEx)
                        {
                            _logger.LogError(shipsterEx, $"Error processing work item {workItem} with CreateShipsterLabelAsync");
                            record.Status = 1;
                            record.Message += " | Error processing Shipster for Label Id:" + workItem + ". " + shipsterEx.Message;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing work item {workItem}");
                        record.Status = 1;
                        record.Message += " | " + ex.Message;
                    }

                    await dbContext.SaveChangesAsync();
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

}