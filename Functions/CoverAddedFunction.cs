using Application.Constants;
using Application.Models;
using Domain.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Functions
{
    public class CoverAddedFunction
    {
        private readonly IAuditingService _auditingService;
        private readonly ILogger<CoverAddedFunction> _logger;

        public CoverAddedFunction(IAuditingService auditingService, ILogger<CoverAddedFunction> logger)
        {
            _auditingService = auditingService;
            _logger = logger;
        }

        [Function(nameof(CoverAddedFunction))]
        public async Task Run([QueueTrigger(QueueName.AuditCoverQueue, Connection = "AzureWebJobsStorage")] CoverAuditModel message)
        {
            await _auditingService.AuditCover(message.CoverId, message.HttpRequestType);
            _logger.LogInformation($"Queue trigger function processed: {message.CoverId}");
        }
    }
}
