using Application.Models;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IClaimService
    {
        public Task<Claim> CreateAsync(CreateClaimModel createClaimModel);
        public Task<IEnumerable<Claim>> GetItemsAsync();
        public Task<Claim> GetItemAsync(string Id);
        public Task DeleteItemAsync(string Id);
    }
}
