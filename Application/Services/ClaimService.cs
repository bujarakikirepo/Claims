using Application.Constants;
using Application.Models;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Domain.Services
{
    public class ClaimService : IClaimService
    {
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
        public async Task<GetClaimModel> CreateAsync(CreateClaimModel createClaimModel)
        {
            //validate input
            var entity = ConvertToEntity(createClaimModel);
            await _claimCosmosDbService.AddItemAsync(entity);
            var claimAuditModel = GetAuditModel(entity.Id, "POST");
            await AddMessageToQueue(JsonSerializer.Serialize(claimAuditModel));
            return CovertToModel(entity);
        }

        public async Task<GetClaimModel> GetItemAsync(string Id)
        {
            var entity = await _claimCosmosDbService.GetItemAsync(Id);
            return CovertToModel(entity);
        }

        public async Task<IEnumerable<GetClaimModel>> GetItemsAsync()
        {
            var entities = await _claimCosmosDbService.GetItemsAsync();
            var models = entities.Select(CovertToModel).ToList();
            return models;
        }

        public async Task DeleteItemAsync(string id)
        {
            await _claimCosmosDbService.DeleteItemAsync(id);
            var claimAuditModel = GetAuditModel(id, "DELETE");
            await AddMessageToQueue(JsonSerializer.Serialize(claimAuditModel));
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

        private async Task AddMessageToQueue(string message)
        {
            await _queueStorageService.UploadNewMessageToQueueAsync(_configuration.GetConnectionString("StorageAccount"),
                QueueName.AuditClaimQueue,
                message);
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

        private static GetClaimModel CovertToModel(Claim claim)
        {
            return new GetClaimModel
            {
                CoverId = claim.CoverId,
                Created = claim.Created,
                DamageCost = claim.DamageCost,
                Id = claim.Id,
                Name = claim.Name,
                Type = claim.Type
            };
        }
    }
}
