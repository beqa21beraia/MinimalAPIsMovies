using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Entities;

namespace MinimalAPIsMovies.Interfaces
{
    public interface IMoviesRepository
    {
        Task<int> CreateAsync(Movie movie);
        Task<List<Movie>> GetAllAsync(PaginationDTO paginationDTO);
        Task<Movie?> GetByIdAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task UpdateAsync(Movie movie);
        Task DeleteAsync(int id);
        Task AssignAsync(int id, List<int> genresIds);
    }
}
