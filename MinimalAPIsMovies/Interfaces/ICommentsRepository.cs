using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Entities;

namespace MinimalAPIsMovies.Interfaces
{
    public interface ICommentsRepository
    {
        Task<int> CreateAsync(Comment comment);
        Task<List<Comment>> GetAllAsync(int movieId);
        Task<Comment?> GetByIdAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task UpdateAsync(Comment comment);
        Task DeleteAsync(int id);
    }
}
