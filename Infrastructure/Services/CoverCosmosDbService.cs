using Application.Interfaces;
using Domain.Entities;
using Microsoft.Azure.Cosmos;

namespace Infrastructure.Services
{
    public class CoverCosmosDbService : ICoverCosmosDbService
    {
        private readonly Container _container;

        public CoverCosmosDbService(CosmosClient dbClient, string databaseName, string containerName)
        {
            if (dbClient == null) throw new ArgumentNullException(nameof(dbClient));
            _container = dbClient.GetContainer(databaseName, containerName);
        }

        public async Task AddItemAsync(Cover item)
        {
            await _container.CreateItemAsync(item, new PartitionKey(item.Id));
        }

        public async Task DeleteItemAsync(string id)
        {
            await _container.DeleteItemAsync<Claim>(id, new PartitionKey(id));
        }

        public async Task<Cover> GetItemAsync(string id)
        {
            try
            {
                var response = await _container.ReadItemAsync<Cover>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<IEnumerable<Cover>> GetItemsAsync()
        {
            var query = _container.GetItemQueryIterator<Cover>(new QueryDefinition("SELECT * FROM c"));
            var results = new List<Cover>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response.ToList());
            }
            return results;
        }
    }
}
