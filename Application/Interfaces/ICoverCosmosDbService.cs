using Domain.Entities;

namespace Application.Interfaces
{
    public interface ICoverCosmosDbService
    {
        Task<IEnumerable<Cover>> GetItemsAsync();
        Task<Cover> GetItemAsync(string id);
        Task AddItemAsync(Cover item);
        Task DeleteItemAsync(string id);
    }
}
