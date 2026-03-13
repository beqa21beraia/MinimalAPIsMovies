using MinimalAPIsMovies.Entities;

namespace MinimalAPIsMovies.Interfaces
{
    public interface IGenresRepository
    {
        Task<int> CreateAsync(Genre genre);
        Task<List<Genre>> GetAllAsync();
        Task<Genre?> GetById(int id);
    }
}
