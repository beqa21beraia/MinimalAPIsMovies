using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MinimalAPIsMovies.Interfaces;
using System.Data;
using System.Security.Claims;

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

        public async Task<IList<Claim>> GetClaimsAsync(IdentityUser identityUser)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var claims = await connection.QueryAsync<Claim>("Users_GetClaims",
                    new { identityUser.Id }, commandType: CommandType.StoredProcedure);
                return claims.ToList();
            }
        }

        public async Task AssignClaimsAsync(IdentityUser identityUser, IEnumerable<Claim> claims)
        {
            var sql = @"INSERT INTO UsersC1aims (UserId, ClaimType, ClaimValue)
                        VALUES (@Id, @Type, @Value)";
            var parameters = claims.Select(c => new { identityUser.Id, c.Type, c.Value });

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(sql, parameters);
            }
        }

        public async Task RemoveClaimsAsync(IdentityUser identityUser, IEnumerable<Claim> claims)
        {
            var sql = @"DELETE UsersC1aims
                        WHERE UserId = @Id AND ClaimType = @Type";
            var parameters = claims.Select(c => new { identityUser.Id, c.Type });

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(sql, parameters);
            }
        }
    }
}
