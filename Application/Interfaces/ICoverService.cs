using Application.Models;
using Domain.Enums;

namespace Application.Interfaces
{
    public interface ICoverService
    {
        public Task<GetCoverModel> CreateAsync(CreateCoverModel createCoverModel);
        public Task<IEnumerable<GetCoverModel>> GetItemsAsync();
        public Task<GetCoverModel> GetItemAsync(string Id);
        public Task DeleteItemAsync(string Id);
        public decimal ComputePremium(DateOnly startDate, DateOnly endDate, CoverType coverType);
    }
}
