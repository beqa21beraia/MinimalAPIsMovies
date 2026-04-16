using Microsoft.AspNetCore.Identity;

namespace MinimalAPIsMovies.Interfaces
{
    public interface IUsersService
    {
        Task<IdentityUser?> GetUserAsync();
    }
}