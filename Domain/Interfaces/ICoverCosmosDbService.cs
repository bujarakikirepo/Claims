using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ICoverCosmosDbService
    {
        Task<IEnumerable<Cover>> GetItemsAsync();
        Task<Cover> GetItemAsync(string id);
        Task AddItemAsync(Cover item);
        Task DeleteItemAsync(string id);
    }
}
