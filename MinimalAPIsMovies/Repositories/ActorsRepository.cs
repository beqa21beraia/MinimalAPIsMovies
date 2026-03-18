using Dapper;
using Microsoft.Data.SqlClient;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Interfaces;
using System.Data;
using System.Diagnostics.Contracts;

namespace MinimalAPIsMovies.Repositories
{
    public class ActorsRepository : IActorsRepository
    {
        private readonly string _connectionString;

        public ActorsRepository(IConfiguration configuration)
        {
            _connectionString = configuration
                 .GetConnectionString("DefaultConnection")!;
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
                await connection.ExecuteAsync("Actors", 
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

        public async Task<List<Actor>> GetAllAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var actors = await connection.QueryAsync<Actor>("Actors_GetAll",
                    commandType: CommandType.StoredProcedure);

                return actors.ToList();
            }
        }

        public async Task<Genre?> GetByIdAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var genre = await connection.QueryFirstOrDefaultAsync<Genre>("Actors_GetById",
                    new {id}, commandType: CommandType.StoredProcedure);

                return genre;
            }
        }

        public async Task UpdateAsync(Actor actor)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("Actors_Update",
                    new { actor.Name, actor.DateOfBirth, actor.Picture },
                    commandType: CommandType.StoredProcedure);
            }
        }
    }
}
