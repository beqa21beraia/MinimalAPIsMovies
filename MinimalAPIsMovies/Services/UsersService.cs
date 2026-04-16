using Microsoft.AspNetCore.Identity;
using MinimalAPIsMovies.Interfaces;

namespace MinimalAPIsMovies.Services
{
    public class UsersService(IHttpContextAccessor httpContextAccessor,
        UserManager<IdentityUser> userManager) : IUsersService
    {
        public async Task<IdentityUser?> GetUserAsync()
        {
            var emailClaim = httpContextAccessor.HttpContext!.User
                .Claims.Where(c => c.Type == "email").FirstOrDefault();

            if (emailClaim == null)
            {
                return null;
            }

            var email = emailClaim.Value;
            return await userManager.FindByEmailAsync(email);
        }
    }
}
