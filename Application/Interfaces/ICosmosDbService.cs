namespace Application.Interfaces
{
    public interface ICosmosDbService<TEntity> where TEntity : class
    {
        Task<IEnumerable<TEntity>> GetItemsAsync();
        Task<TEntity> GetItemAsync(string id);
        Task AddItemAsync(TEntity item);
        Task DeleteItemAsync(string id);
    }
}
