using Application.Constants;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Functions
{
    public class ClaimAddedFunction
    {
        private readonly ILogger<ClaimAddedFunction> _logger;
        private readonly IAuditingService _auditingService;

        public ClaimAddedFunction(ILogger<ClaimAddedFunction> logger, IAuditingService auditingService)
        {
            _logger = logger;
            _auditingService = auditingService;
        }

        [Function(nameof(ClaimAddedFunction))]
        public async Task Run([QueueTrigger(QueueName.AuditClaimQueue, Connection = "AzureWebJobsStorage")] ClaimAuditModel message)
        {
            await _auditingService.AuditClaim(message.ClaimId, message.HttpRequestType);
            _logger.LogInformation($"Queue trigger function processed: {message.ClaimId}");
        }
    }
}
