using Application.Constants;
using Application.Exceptions;
using Application.Models;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Text.Json;

namespace Application.Tests.Services
{
    [TestFixture]
    public class CoverServiceTests
    {
        [Test]
        public void GIVEN_InvalidStartDate_WHEN_CreateAsync_THEN_ThrowsBadRequestException()
        {
            // Arrange
            var createCoverModel = new CreateCoverModel
            {
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
                Type = CoverType.Yacht
            };

            var coverCosmosDbServiceMock = new Mock<ICoverCosmosDbService>();
            var queueStorageServiceMock = new Mock<IQueueStorageService>();
            var configurationMock = new Mock<IConfiguration>();

            var coverService = new CoverService(
                coverCosmosDbServiceMock.Object,
                queueStorageServiceMock.Object,
                configurationMock.Object
            );

            // Act
            var exception = Assert.ThrowsAsync<BadRequestException>(async () =>
                await coverService.CreateAsync(createCoverModel));

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(exception, Is.Not.Null);
                Assert.That(exception.Message, Is.EqualTo("StartDate cannot be in the past"));
            });
        }

        [Test]
        public void GIVEN_InvalidInsurancePeriod_WHEN_CreateAsync_THEN_ThrowsBadRequestException()
        {
            // Arrange
            var createCoverModel = new CreateCoverModel
            {
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(366)),
                Type = Domain.Enums.CoverType.Yacht
            };

            var coverCosmosDbServiceMock = new Mock<ICoverCosmosDbService>();
            var queueStorageServiceMock = new Mock<IQueueStorageService>();
            var configurationMock = new Mock<IConfiguration>();

            var coverService = new CoverService(
                coverCosmosDbServiceMock.Object,
                queueStorageServiceMock.Object,
                configurationMock.Object
            );

            // Act
            var exception = Assert.ThrowsAsync<BadRequestException>(async () =>
                await coverService.CreateAsync(createCoverModel));

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(exception, Is.Not.Null);
                Assert.That(exception.Message, Is.EqualTo("Total insurance period cannot exceed 1 year"));
            });
        }

        [Test]
        public async Task GIVEN_ValidInput_WHEN_CreateAsync_THEN_CreatesCoverAndAddsMessageToQueue()
        {
            // Arrange
            var createCoverModel = new CreateCoverModel
            {
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
                Type = CoverType.Yacht
            };

            var coverCosmosDbServiceMock = new Mock<ICoverCosmosDbService>();
            var queueStorageServiceMock = new Mock<IQueueStorageService>();

            var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:StorageAccount", "Dev" }
            })
            .Build();

            var coverService = new CoverService(
                coverCosmosDbServiceMock.Object,
                queueStorageServiceMock.Object,
                configuration
            );

            // Act
            var result = await coverService.CreateAsync(createCoverModel);

            // Assert
            coverCosmosDbServiceMock.Verify(
                x => x.AddItemAsync(It.Is<Cover>(c => c.StartDate == createCoverModel.StartDate
                                                      && c.EndDate == createCoverModel.EndDate
                                                      && c.Type == createCoverModel.Type)),
                Times.Once
            );

            queueStorageServiceMock.Verify(
                x => x.UploadNewMessageToQueueAsync(It.IsAny<string>(), QueueName.AuditCoverQueue, It.IsAny<string>()),
                Times.Once
            );

            var capturedMessage = GetCapturedQueueMessage(queueStorageServiceMock);

            Assert.That(capturedMessage, Is.Not.Null.And.Not.Empty);

            var auditModel = JsonSerializer.Deserialize<CoverAuditModel>(capturedMessage);
            Assert.Multiple(() =>
            {
                Assert.That(auditModel?.CoverId, Is.EqualTo(result.Id));
                Assert.That(auditModel?.HttpRequestType, Is.EqualTo("POST"));
            });
        }

        [TestCase(CoverType.Yacht)]
        [TestCase(CoverType.Tanker)]
        [TestCase(CoverType.PassengerShip)]
        [TestCase(CoverType.BulkCarrier)]
        [TestCase(CoverType.ContainerShip)]
        public void Given30DaysInsuranceLength_WHEN_ComputePremium_THEN_ReturnsCorrectPremium(CoverType coverType)
        {
            // Arrange
            var coverCosmosDbServiceMock = new Mock<ICoverCosmosDbService>();
            var queueStorageServiceMock = new Mock<IQueueStorageService>();
            var configurationMock = new Mock<IConfiguration>();

            var coverService = new CoverService(
                coverCosmosDbServiceMock.Object,
                queueStorageServiceMock.Object,
                configurationMock.Object
            );

            var startDate = new DateOnly(2023, 1, 1);
            var endDate = new DateOnly(2023, 1, 31);

            // Act
            var result = coverService.ComputePremium(startDate, endDate, coverType);

            // Assert
            var basePremiumPerDay = 1250m;
            var multiplier = GetMultiplier(coverType);
            var premiumPerDay = basePremiumPerDay * multiplier;
            var expectedPremium = premiumPerDay * 30;

            Assert.That(result, Is.EqualTo(expectedPremium).Within(0.01m));
        }

        [Test]
        public void Given180DaysInsuranceLength_WHEN_ComputePremium_THEN_ReturnsCorrectPremium()
        {
            // Arrange
            var coverCosmosDbServiceMock = new Mock<ICoverCosmosDbService>();
            var queueStorageServiceMock = new Mock<IQueueStorageService>();
            var configurationMock = new Mock<IConfiguration>();

            var coverService = new CoverService(
                coverCosmosDbServiceMock.Object,
                queueStorageServiceMock.Object,
                configurationMock.Object
            );

            var startDate = new DateOnly(2023, 1, 1);
            var endDate = startDate.AddDays(180);
            var coverType = CoverType.Yacht;

            // Act
            var result = coverService.ComputePremium(startDate, endDate, coverType);

            // Assert
            Assert.That(result, Is.EqualTo(237256.250m).Within(0.01m));
        }

        [Test]
        public void Given365DaysInsuranceLength_WHEN_ComputePremium_THEN_ReturnsCorrectPremium()
        {
            // Arrange
            var coverCosmosDbServiceMock = new Mock<ICoverCosmosDbService>();
            var queueStorageServiceMock = new Mock<IQueueStorageService>();
            var configurationMock = new Mock<IConfiguration>();

            var coverService = new CoverService(
                coverCosmosDbServiceMock.Object,
                queueStorageServiceMock.Object,
                configurationMock.Object
            );

            var startDate = new DateOnly(2023, 1, 1);
            var endDate = startDate.AddDays(365);
            var coverType = CoverType.Yacht;

            // Act
            var result = coverService.ComputePremium(startDate, endDate, coverType);

            // Assert
            Assert.That(result, Is.EqualTo(484000.000m).Within(0.01m));
        }

        private static decimal GetMultiplier(CoverType coverType)
        {
            switch (coverType)
            {
                case CoverType.Yacht:
                    return 1.1m;
                case CoverType.PassengerShip:
                    return 1.2m;
                case CoverType.Tanker:
                    return 1.5m;
                default:
                    return 1.3m;
            }
        }

        private static string GetCapturedQueueMessage(Mock<IQueueStorageService> queueStorageServiceMock)
        {
            var capturedArguments = (string)queueStorageServiceMock.Invocations[0].Arguments[2];
            return capturedArguments;
        }
    }
}
