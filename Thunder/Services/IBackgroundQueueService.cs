using Azure.Storage.Queues;
using Azure;
using Azure.Storage.Queues.Models;
using Azure.Security.KeyVault.Secrets;
using System.Security.Policy;
using Thunder.Models;

//when a new label is created, the webhook sends a message to the app with the label's unique ID.

//application receives this unique label ID and queues it for further processing. It does this by adding it to the "createlabelqueue" in Azure Storage.

//The CreateLabelBackgroundService is constantly monitoring the "createlabelqueue".
//When it detects that a new item(the label ID) has been added, it dequeues the item and starts the process of creating an AIOLabel or a ShipsterLabel.
//The process is as follows:

//It tries to create an AIOLabel first. If it is successful, it logs a success message and updates the database record for the label.
//The status of the label is set to "3" (finished processing), and a success message is added.

//If the AIOLabel creation fails, it logs the error and attempts to create a ShipsterLabel.
//If this is successful, it logs a success message and updates the database record for the label.
//The status is set to "3" (finished processing), and a success message is added.

//If the ShipsterLabel creation also fails, it logs the error and updates the database record for the label.
//The status is set to "1" (error occurred), and the error message is added.

//Once the label has been successfully created, the label ID is then added to the "retrievestorequeue" in Azure Storage.

//The RetrieveAndStoreLabelBackgroundService is constantly monitoring the "retrievestorequeue". When it detects that a new item(the label ID) has been added,
//it dequeues the item and starts the process of retrieving the label and storing it in a blob storage.
//If successful, it logs a success message and updates the database record for the label with a status of "3" (finished processing) and a success message.
//If it fails, it logs the error and updates the database record with a status of "1" (error occurred) and the error message.

//This flow allows you to process the creation and storage of labels in the background,
//freeing up resources for other tasks. It also provides a way to handle errors and retry operations in a controlled manner.

namespace Thunder.Services
{
    public interface IBackgroundQueueService
    {
        void EnqueueCreateLabel(string workItem);
        Task<string> DequeueCreateLabel(CancellationToken cancellationToken);

        void EnqueueRetrieveStore(string workItem);
        Task<string> DequeueRetrieveStore(CancellationToken cancellationToken);
    }

    public class BackgroundQueueService : IBackgroundQueueService
    {
        private readonly QueueClient _createLabelQueueClient;
        private readonly QueueClient _retrieveStoreQueueClient;

        public BackgroundQueueService(SecretClient secretClient, string createLabelQueueName, string retrieveStoreQueueName)
        {
            var blobStorageSecret = secretClient.GetSecret("thunderblobstorage");
            _createLabelQueueClient = new QueueClient(blobStorageSecret.Value.Value, createLabelQueueName);
            _retrieveStoreQueueClient = new QueueClient(blobStorageSecret.Value.Value, retrieveStoreQueueName);
        }

        public void EnqueueCreateLabel(string workItem)
        {
            if (workItem == null)
                throw new ArgumentNullException(nameof(workItem));

            _createLabelQueueClient.SendMessage(workItem);
        }

        public async Task<string> DequeueCreateLabel(CancellationToken cancellationToken)
        {
            return await DequeueAsync(_createLabelQueueClient, cancellationToken);
        }

        public void EnqueueRetrieveStore(string workItem)
        {
            if (workItem == null)
                throw new ArgumentNullException(nameof(workItem));

            _retrieveStoreQueueClient.SendMessage(workItem);
        }

        public async Task<string> DequeueRetrieveStore(CancellationToken cancellationToken)
        {
            return await DequeueAsync(_retrieveStoreQueueClient, cancellationToken);
        }

        private async Task<string> DequeueAsync(QueueClient queueClient, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            Response<QueueMessage[]> response = await queueClient.ReceiveMessagesAsync();

            if (response.Value.Length > 0)
            {
                QueueMessage message = response.Value[0];
                await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                return message.MessageText;
            }

            return null;
        }
    }
}