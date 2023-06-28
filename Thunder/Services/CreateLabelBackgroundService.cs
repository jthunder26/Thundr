using System.Text;
using Thunder.Data;
using Thunder.Models;

namespace Thunder.Services
{
    public class CreateLabelBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public CreateLabelBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
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

                    var dequeueResult = await backgroundQueueService.DequeueCreateLabel(stoppingToken);
                    string base64WorkItem = dequeueResult.Item1;
                    string messageId = dequeueResult.Item2;
                    string popReceipt = dequeueResult.Item3;

                    if (base64WorkItem == null) continue;

                    // Decode the Base64-encoded work item to a string.
                    byte[] data = Convert.FromBase64String(base64WorkItem);
                    string decodedWorkItem = Encoding.UTF8.GetString(data);

                    // Now convert the string to an int.
                    int workItemId = int.Parse(decodedWorkItem);

                    LabelDetail record = dbContext.LabelDetail.FirstOrDefault(x => x.LabelId == workItemId);
                    if (record != null && record.Status < 2)
                    {
                        try
                        {
                            await TryCreateAIOLabel(thunderService, backgroundQueueService, workItemId, record);
                            backgroundQueueService.ConfirmCreateLabel(messageId, popReceipt);
                        }
                        catch (ApplicationException ex)
                        {
                            await HandleAIOError(thunderService, backgroundQueueService, workItemId, record, ex, messageId, popReceipt);
                        }
                        catch (Exception ex)
                        {
                            HandleGeneralError(record, workItemId, ex);
                        }

                        await dbContext.SaveChangesAsync();
                    }
                }

                
            }
        }

        private async Task TryCreateAIOLabel(IThunderService thunderService, IBackgroundQueueService backgroundQueueService, int workItemId, LabelDetail record)
        {
            await thunderService.CreateAIOLabelAsync(workItemId);
            record.Status = 3;
            record.AIO_Attempt++;
            record.Error = 0;
            record.Message = "TryCreateAIOLabel: Successfully processed AIO Label with Id: " + workItemId;

            backgroundQueueService.EnqueueRetrieveStore(workItemId.ToString());
        }

        private async Task HandleAIOError(IThunderService thunderService, IBackgroundQueueService backgroundQueueService, int workItemId, LabelDetail record, ApplicationException ex, string messageId, string popReceipt)
        {
            record.Status = 1;
            record.Error = 1;
            record.ErrorMsg = "HandleAIOError: Error processing AIO for Label Id:" + workItemId + ". " + ex.Message;

            try
            {
                await thunderService.CreateShipsterLabelAsync(workItemId);
                record.Status = 3;
                record.Shipster_Attempt++;
                record.Error = 0;
                record.Message += " | HandleAIOError:CreateShipsterLabelAsync: Successfully processed Shipster Label with Id: " + workItemId;

                backgroundQueueService.EnqueueRetrieveStore(workItemId.ToString());
                backgroundQueueService.ConfirmCreateLabel(messageId, popReceipt); // Delete the message from create label queue.
            }
            catch (Exception shipsterEx)
            {
                HandleShipsterError(record, workItemId, shipsterEx);
            }
        }

        private void HandleShipsterError(LabelDetail record, int workItemId, Exception shipsterEx)
        {
            record.Status = 1;
            record.Error = 1;
            record.ErrorMsg += " | HandleShipsterError: Error processing Shipster for Label Id:" + workItemId + ". " + shipsterEx.Message;
        }

        private void HandleGeneralError(LabelDetail record, int workItemId, Exception ex)
        {
            record.Status = 1;
            record.Error = 1;
            record.ErrorMsg += " | HandleGeneralError:" + ex.Message;
        }
    }
}
