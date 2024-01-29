using Application.Constants;
using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Application.Services
{
    public class CoverService : ICoverService
    {
        private readonly ICoverCosmosDbService _coverCosmosDbService;
        private readonly IQueueStorageService _queueStorageService;
        private readonly IConfiguration _configuration;

        public CoverService(ICoverCosmosDbService coverCosmosDbService,
            IQueueStorageService queueStorageService,
            IConfiguration configuration)
        {
            _coverCosmosDbService = coverCosmosDbService;
            _queueStorageService = queueStorageService;
            _configuration = configuration;

        }
        public async Task<GetCoverModel> CreateAsync(CreateCoverModel createCoverModel)
        {
            //validate input
            var entity = ConvertToEntity(createCoverModel);
            await _coverCosmosDbService.AddItemAsync(entity);
            var claimAuditModel = GetAuditModel(entity.Id, "POST");
            await AddMessageToQueue(JsonSerializer.Serialize(claimAuditModel));
            return ConvertToModel(entity);
        }

        public async Task<GetCoverModel> GetItemAsync(string Id)
        {
            var entity = await _coverCosmosDbService.GetItemAsync(Id);
            return ConvertToModel(entity);
        }

        public async Task<IEnumerable<GetCoverModel>> GetItemsAsync()
        {
            var entities = await _coverCosmosDbService.GetItemsAsync();
            var models = entities.Select(ConvertToModel).ToList();
            return models;
        }

        public async Task DeleteItemAsync(string id)
        {
            await _coverCosmosDbService.DeleteItemAsync(id);
            var claimAuditModel = GetAuditModel(id, "DELETE");
            await AddMessageToQueue(JsonSerializer.Serialize(claimAuditModel));
        }

        private async Task AddMessageToQueue(string message)
        {
            await _queueStorageService.UploadNewMessageToQueueAsync(_configuration.GetConnectionString("StorageAccount"),
                QueueName.AuditCoverQueue,
                message);
        }

        public decimal ComputePremium(DateOnly startDate, DateOnly endDate, CoverType coverType)
        {
            var multiplier = 1.3m;
            if (coverType == CoverType.Yacht)
            {
                multiplier = 1.1m;
            }

            if (coverType == CoverType.PassengerShip)
            {
                multiplier = 1.2m;
            }

            if (coverType == CoverType.Tanker)
            {
                multiplier = 1.5m;
            }

            var premiumPerDay = 1250 * multiplier;
            var insuranceLength = endDate.DayNumber - startDate.DayNumber;
            var totalPremium = 0m;

            for (var i = 0; i < insuranceLength; i++)
            {
                if (i < 30) totalPremium += premiumPerDay;
                if (i < 180 && coverType == CoverType.Yacht) totalPremium += premiumPerDay - premiumPerDay * 0.05m;
                else if (i < 180) totalPremium += premiumPerDay - premiumPerDay * 0.02m;
                if (i < 365 && coverType != CoverType.Yacht) totalPremium += premiumPerDay - premiumPerDay * 0.03m;
                else if (i < 365) totalPremium += premiumPerDay - premiumPerDay * 0.08m;
            }

            return totalPremium;
        }

        private static CoverAuditModel GetAuditModel(string id, string requestType)
        {
            return new CoverAuditModel
            {
                CoverId = id,
                Created = DateTime.UtcNow,
                HttpRequestType = requestType
            };
        }

        private Cover ConvertToEntity(CreateCoverModel createCoverModel)
        {
            return new Cover
            {
                Id = Guid.NewGuid().ToString(),
                StartDate = createCoverModel.StartDate,
                EndDate = createCoverModel.EndDate,
                Premium = ComputePremium(createCoverModel.StartDate, createCoverModel.EndDate, createCoverModel.Type),
                Type = createCoverModel.Type,
            };
        }

        private static GetCoverModel ConvertToModel(Cover cover)
        {
            return new GetCoverModel
            {
                Id = cover.Id,
                StartDate = cover.StartDate,
                EndDate = cover.EndDate,
                Premium = cover.Premium,
                Type = cover.Type,
            };
        }
    }
}
