using Application.Models;
using Domain.Models;

namespace Domain.Interfaces
{
    public interface IClaimService
    {
        public Task<GetClaimModel> CreateAsync(CreateClaimModel createClaimModel);
        public Task<IEnumerable<GetClaimModel>> GetItemsAsync();
        public Task<GetClaimModel> GetItemAsync(string Id);
        public Task DeleteItemAsync(string Id);
    }
}
