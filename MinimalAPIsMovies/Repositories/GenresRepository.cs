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

        public async Task<Genre?> GetById(int id)
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
    }
}