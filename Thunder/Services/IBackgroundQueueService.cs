using Azure.Storage.Queues;
using Azure;
using Azure.Storage.Queues.Models;
using Azure.Security.KeyVault.Secrets;
using System.Security.Policy;
using Thunder.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Thunder.Services
{
    public interface IBackgroundQueueService
    {
        void EnqueueCreateLabel(string workItem);
        Task<(string, string, string)> DequeueCreateLabel(CancellationToken cancellationToken);
        void ConfirmCreateLabel(string messageId, string popReceipt); // Add this line
        void EnqueueRetrieveStore(string workItem);
        Task<(string, string, string)> DequeueRetrieveStore(CancellationToken cancellationToken);
        void ConfirmRetrieveStore(string messageId, string popReceipt);
    }
    /// <summary>
    /// In this updated code, we maintain a ConcurrentDictionary _failedWorkItems to keep track of failed work items.
    /// When a work item fails, you can call the MarkWorkItemAsFailed method, passing the work item. 
    /// This will add the work item to _failedWorkItems with the current UTC time as the value.
    /// In the EnqueueCreateLabel and EnqueueRetrieveStore methods, we first check if the work item is in _failedWorkItems.
    /// If it is and it was added more than 10 minutes ago, we remove it from _failedWorkItems and proceed to enqueue it.
    /// If it is in _failedWorkItems and it was added less than 10 minutes ago, we do not enqueue it.
    /// 
    /// </summary>
    public class BackgroundQueueService : IBackgroundQueueService
    {
        private readonly QueueClient _createLabelQueueClient;
        private readonly QueueClient _retrieveStoreQueueClient;
        private readonly ConcurrentDictionary<string, DateTime> _failedWorkItems = new ConcurrentDictionary<string, DateTime>();

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

            if (_failedWorkItems.ContainsKey(workItem))
            {
                if (_failedWorkItems[workItem] < DateTime.UtcNow.AddMinutes(-10))
                {
                    _failedWorkItems.TryRemove(workItem, out _);
                }
                else
                {
                    return;
                }
            }

            _createLabelQueueClient.SendMessage(workItem);
        }

        public async Task<(string, string, string)> DequeueCreateLabel(CancellationToken cancellationToken)
        {
            return await DequeueAsync(_createLabelQueueClient, cancellationToken);
        }
        public void ConfirmCreateLabel(string messageId, string popReceipt)
        {
            _createLabelQueueClient.DeleteMessage(messageId, popReceipt);
        }
        public void EnqueueRetrieveStore(string workItem)
        {
            if (workItem == null)
                throw new ArgumentNullException(nameof(workItem));

            if (_failedWorkItems.ContainsKey(workItem))
            {
                if (_failedWorkItems[workItem] < DateTime.UtcNow.AddMinutes(-10))
                {
                    _failedWorkItems.TryRemove(workItem, out _);
                }
                else
                {
                    return;
                }
            }

            _retrieveStoreQueueClient.SendMessage(workItem);
        }

        public async Task<(string, string, string)> DequeueRetrieveStore(CancellationToken cancellationToken)
        {
            return await DequeueAsync(_retrieveStoreQueueClient, cancellationToken);
        }

        public void ConfirmRetrieveStore(string messageId, string popReceipt)
        {
            _retrieveStoreQueueClient.DeleteMessage(messageId, popReceipt);
        }

        public void MarkWorkItemAsFailed(string workItem)
        {
            _failedWorkItems[workItem] = DateTime.UtcNow;
        }

        private async Task<(string, string, string)> DequeueAsync(QueueClient queueClient, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return (null, null, null);
            }

            Response<QueueMessage[]> response = await queueClient.ReceiveMessagesAsync();

            if (response.Value.Length > 0)
            {
                return (response.Value[0].MessageText, response.Value[0].MessageId, response.Value[0].PopReceipt);
            }
            else
            {
                return (null, null, null);
            }
        }
    }
}
