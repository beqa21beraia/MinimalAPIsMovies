using Dapper;
using Microsoft.Data.SqlClient;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Interfaces;
using System.Data;

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
                var id = await connection.QuerySingleAsync<int>("Genres_Create",
                    new {genre.Name}, commandType: CommandType.StoredProcedure);
                
                genre.Id = id;
                return id;
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("Genres_Delete",
                    new { id }, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var exists = await connection.QuerySingleAsync<bool>("Genres_Exists",
                    new { id }, commandType: CommandType.StoredProcedure);

                return exists;
            }
        }

        public async Task<List<int>> ExistsAsync(List<int> genresIds)
        {
            var dt = new DataTable();
            dt.Columns.Add("Id", typeof(int));

            foreach (int genreId in genresIds)
            {
                dt.Rows.Add(genreId);
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                var idsOfGenresThatExists = await connection
                    .QueryAsync<int>("Genres_GetBySeveralIds", new { genresIds = dt },
                    commandType: CommandType.StoredProcedure);

                return idsOfGenresThatExists.ToList();
            }
        }

        public async Task<List<Genre>> GetAllAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var genres = await connection.QueryAsync<Genre>("Genres_GetAll",
                    commandType: CommandType.StoredProcedure);

                return genres.ToList();
            }
        }

        public async Task<Genre?> GetByIdAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var genre = await connection.QueryFirstOrDefaultAsync<Genre>("Genres_GetById",
                    new {id}, commandType: CommandType.StoredProcedure);

                return genre;
            }            
        }

        public async Task UpdateAsync(Genre genre)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("Genres_Update", 
                    new {genre.Id, genre.Name}, 
                    commandType: CommandType.StoredProcedure);
            }
        }
    }
}