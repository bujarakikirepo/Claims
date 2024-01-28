using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Application.Services
{
    public class ClaimService : IClaimService
    {
        const string QueueName = "audit";
        private readonly IClaimCosmosDbService _claimCosmosDbService;
        private readonly IQueueStorageService _queueStorageService;
        private readonly IConfiguration _configuration;

        public ClaimService(IClaimCosmosDbService claimCosmosDbService,
            IQueueStorageService queueStorageService,
            IConfiguration configuration)
        {
            _claimCosmosDbService = claimCosmosDbService;
            _queueStorageService = queueStorageService;
            _configuration = configuration;

        }
        public async Task<Claim> CreateAsync(CreateClaimModel createClaimModel)
        {
            //validate input
            var entity = ConvertToEntity(createClaimModel);
            await _claimCosmosDbService.AddItemAsync(entity);
            var claimAuditModel = GetAuditModel(entity.Id, "POST");
            var message = JsonSerializer.Serialize(claimAuditModel);
            await _queueStorageService.UploadNewMessageToQueueAsync(_configuration.GetConnectionString("StorageAccount"), QueueName, message);
            return entity;
        }

        public async Task<Claim> GetItemAsync(string Id)
        {
            return await _claimCosmosDbService.GetItemAsync(Id);
        }

        public async Task<IEnumerable<Claim>> GetItemsAsync()
        {
            return await _claimCosmosDbService.GetItemsAsync();
        }

        public async Task DeleteItemAsync(string id)
        {
            var claimAuditModel = GetAuditModel(id, "DELETE");
            await _claimCosmosDbService.DeleteItemAsync(id);
        }

        private static ClaimAuditModel GetAuditModel(string id, string requestType)
        {
            return new ClaimAuditModel
            {
                ClaimId = id,
                Created = DateTime.UtcNow,
                HttpRequestType = requestType
            };
        }

        private static Claim ConvertToEntity(CreateClaimModel createClaimModel)
        {
            return new Claim
            {
                Id = Guid.NewGuid().ToString(),
                CoverId = createClaimModel.CoverId,
                Created = DateTime.UtcNow,
                DamageCost = createClaimModel.DamageCost,
                Name = createClaimModel.Name,
                Type = createClaimModel.Type,
            };
        }
    }
}
