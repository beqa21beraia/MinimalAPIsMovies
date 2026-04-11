using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MinimalAPIsMovies.Interfaces;
using System.Data;

namespace MinimalAPIsMovies.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private readonly string? _connectionString;

        public UsersRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IdentityUser?> GetByEmailAsync(string normalizedEmail)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QuerySingleOrDefaultAsync<IdentityUser>(
                    "Users_GetByEmail", new { normalizedEmail }, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<string> CreateAsync(IdentityUser user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                user.Id = Guid.NewGuid().ToString();
                await connection.ExecuteAsync("Users_Create", new
                {
                    user.Id,
                    user.Email,
                    user.NormalizedEmail,
                    user.UserName,
                    user.NormalizedUserName,
                    user.PasswordHash
                }, commandType: CommandType.StoredProcedure);

                return user.Id;
            }
        }
    }
}
