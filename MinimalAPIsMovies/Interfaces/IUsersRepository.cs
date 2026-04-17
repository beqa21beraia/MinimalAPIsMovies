using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace MinimalAPIsMovies.Interfaces
{
    public interface IUsersRepository
    {
        Task AssignClaimsAsync(IdentityUser identityUser, IEnumerable<Claim> claims);
        Task<string> CreateAsync(IdentityUser user);
        Task<IdentityUser?> GetByEmailAsync(string normalizedEmail);
        Task<IList<Claim>> GetClaimsAsync(IdentityUser identityUser);
        Task RemoveClaimsAsync(IdentityUser identityUser, IEnumerable<Claim> claims);
    }
}