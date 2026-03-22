using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Entities;

namespace MinimalAPIsMovies.Interfaces
{
    public interface IActorsRepository
    {
        Task<int> CreateAsync(Actor actor);
        Task<List<Actor>> GetAllAsync(PaginationDTO paginationDTO);
        Task<Actor?> GetByIdAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task UpdateAsync(Actor actor);
        Task DeleteAsync(int id);
        Task<List<Actor>> GetByNameAsync(string name);
    }
}
