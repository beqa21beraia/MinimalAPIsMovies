using MinimalAPIsMovies.Entities;

namespace MinimalAPIsMovies.Interfaces
{
    public interface IGenresRepository
    {
        Task<int> CreateAsync(Genre genre);
        Task<List<Genre>> GetAllAsync();
        Task<Genre?> GetByIdAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task UpdateAsync(Genre genre);
        Task DeleteAsync(int id);
    }
}
