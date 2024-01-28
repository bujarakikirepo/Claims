﻿namespace Application.Interfaces
{
    public interface IQueueStorageService
    {
        Task UploadNewMessageToQueueAsync(string connectionString, string queueName, string message);
    }
}
