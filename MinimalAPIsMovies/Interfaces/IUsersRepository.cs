using Microsoft.AspNetCore.Identity;

namespace MinimalAPIsMovies.Interfaces
{
    public interface IUsersRepository
    {
        Task<string> CreateAsync(IdentityUser user);
        Task<IdentityUser?> GetByEmailAsync(string normalizedEmail);
    }
}