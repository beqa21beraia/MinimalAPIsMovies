using MinimalAPIsMovies.Entities;

namespace MinimalAPIsMovies.Interfaces
{
    public interface IErrorsRepository
    {
        Task<Guid> CreateAsync(Error error);
    }
}