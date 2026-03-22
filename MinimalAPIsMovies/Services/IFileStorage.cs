namespace MinimalAPIsMovies.Services
{
    public interface IFileStorage
    {
        Task<string> StoreAsync(string container, IFormFile file);
        Task DeleteAsync(string? route, string container);
        async Task<string> EditAsync(string? route, string container, IFormFile file)
        {
            await DeleteAsync(route, container);
            return await StoreAsync(container, file);
        }
    }
}
