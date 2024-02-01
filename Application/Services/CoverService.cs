using Application.ComputePremium;
using Application.Constants;
using Application.Exceptions;
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
            ValidateForCreate(createCoverModel);
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
            _ = await _coverCosmosDbService.GetItemAsync(id)
                ?? throw new EntityNotFoundException($"Cover id {id} not found");
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
            var premiumInstance = GetPremiumInstance(coverType);
            return premiumInstance == null
                ? throw new BadRequestException($"Wrong cover type {coverType}")
                : premiumInstance.Compute(startDate, endDate);
        }

        private static void ValidateForCreate(CreateCoverModel createCoverModel)
        {
            if (createCoverModel.StartDate < DateOnly.FromDateTime(DateTime.Now))
            {
                throw new BadRequestException("StartDate cannot be in the past");
            }

            var insuranceLength = createCoverModel.EndDate.DayNumber - createCoverModel.StartDate.DayNumber;
            if (insuranceLength > 365)
            {
                throw new BadRequestException("Total insurance period cannot exceed 1 year");
            }
        }

        private static IComputePremium? GetPremiumInstance(CoverType coverType)
        {
            switch (coverType)
            {
                case CoverType.Yacht:
                    return new YachtComputation();
                case CoverType.PassengerShip:
                    return new PassengerShipComputation();
                case CoverType.Tanker:
                    return new TankerComputation();
                case CoverType.BulkCarrier:
                    return new BulkCarrierComputation();
                case CoverType.ContainerShip:
                    return new ContainerShipComputation();
                default:
                    return null;
            }
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

        //Auto mapper can be used instead of manually 

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
