using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Interfaces;
using System.Data;
using System.Diagnostics.Contracts;

namespace MinimalAPIsMovies.Repositories
{
    public class ActorsRepository : IActorsRepository
    {
        private readonly string _connectionString;
        private readonly HttpContext _httpContext;

        public ActorsRepository(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _connectionString = configuration
                 .GetConnectionString("DefaultConnection")!;
            _httpContext = httpContextAccessor.HttpContext!;
        }
        public async Task<int> CreateAsync(Actor actor)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var id = await connection.QuerySingleAsync<int>("Actors_Create",
                    new { actor.Name, actor.DateOfBirth, actor.Picture },
                    commandType: CommandType.StoredProcedure);

                actor.Id = id;
                return id;
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("Actors_Delete", 
                    new { id }, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var exists = await connection.QuerySingleAsync<bool>("Actors_Exists",
                    new { id }, commandType: CommandType.StoredProcedure);

                return exists;
            }
        }

        public async Task<List<Actor>> GetAllAsync(PaginationDTO pagination)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var actors = await connection.QueryAsync<Actor>("Actors_GetAll",
                    new {pagination.Page, pagination.recordsPerPage},
                    commandType: CommandType.StoredProcedure);
                
                var actorsCount = await connection.QuerySingleAsync<int>("Actors_Count",
                    commandType: CommandType.StoredProcedure);

                _httpContext.Response.Headers.Append("totalAmountOfRecords", actorsCount.ToString());

                return actors.ToList();
            }
        }

        public async Task<Actor?> GetByIdAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var actor = await connection.QueryFirstOrDefaultAsync<Actor>("Actors_GetById",
                    new {id}, commandType: CommandType.StoredProcedure);

                return actor;
            }
        }

        public async Task UpdateAsync(Actor actor)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("Actors_Update",
                    new { actor.Id, actor.Name, actor.DateOfBirth, actor.Picture },
                    commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<List<Actor>> GetByNameAsync(string name)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var actors = await connection.QueryAsync<Actor>("Actors_GetByName",
                    new { name }, commandType: CommandType.StoredProcedure);

                return actors.ToList();
            }
        }
    }
}
