using Domain.Entities;

namespace Application.Interfaces
{
    public interface IClaimCosmosDbService
    {
        Task<IEnumerable<Claim>> GetItemsAsync();
        Task<Claim> GetItemAsync(string id);
        Task AddItemAsync(Claim item);
        Task DeleteItemAsync(string id);
    }
}
