using Dapper;
using Microsoft.Data.SqlClient;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Interfaces;

namespace MinimalAPIsMovies.Repositories
{
    public class GenresRepository : IGenresRepository
    {
        private readonly string _connectionString;

        public GenresRepository(IConfiguration configuration)
        {
            _connectionString = configuration
                .GetConnectionString("DefaultConnection")!;
        }

        public async Task<int> CreateAsync(Genre genre)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                            INSERT INTO Genres (Name)
                            VALUES (@Name);

                            SELECT SCOPE_IDENTITY();
                            ";

                var id = await connection.QuerySingleAsync<int>(query, genre);
                genre.Id = id;
                return id;
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"DELETE Genres
                            WHERE Id = @Id";

                await connection.ExecuteAsync(query, new { id });
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"if exists (select 1 from Genres where Id = @Id)
	                        select 1
                        else 
	                        select 0";

                var exists = await connection.QuerySingleAsync<bool>(query, new { id });

                return exists;
            }
        }

        public async Task<List<Genre>> GetAllAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"SELECT Id, Name 
                              FROM Genres";

                var genres = await connection.QueryAsync<Genre>(query);

                return genres.ToList();
            }
        }

        public async Task<Genre?> GetByIdAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"SELECT Id, Name
                              FROM Genres
                              WHERE Id = @Id";

                var genre = await connection.QueryFirstOrDefaultAsync<Genre>(query, new {id});

                return genre;
            }            
        }

        public async Task UpdateAsync(Genre genre)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"UPDATE Genres
                              SET Name = @Name
                              WHERE Id = @Id";
                
                await connection.ExecuteAsync(query, genre);
            }
        }
    }
}