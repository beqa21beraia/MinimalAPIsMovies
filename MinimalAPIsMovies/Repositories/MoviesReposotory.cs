using Dapper;
using Microsoft.Data.SqlClient;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Interfaces;
using System.Data;

namespace MinimalAPIsMovies.Repositories
{
    public class MoviesReposotory : IMoviesRepository
    {
        private readonly string _connectionString;
        private readonly HttpContext _httpContext;

        public MoviesReposotory(IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _httpContext = httpContextAccessor.HttpContext!;
        }

        public async Task<int> CreateAsync(Movie movie)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var id = await connection.QuerySingleAsync<int>("Movies_Create",
                    new { movie.Title, movie.InTheaters, movie.ReleaseDate, movie.Poster }, 
                    commandType: CommandType.StoredProcedure); 
                movie.Id = id;

                return id;
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("Movies_Delete",
                    commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var exists = await connection.QuerySingleAsync<bool>("Movies_Exists",
                    new { id }, commandType: CommandType.StoredProcedure);

                return exists;
            }
        }

        public async Task<List<Movie>> GetAllAsync(PaginationDTO paginationDTO)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var movies = await connection.QueryAsync<Movie>("Movies_GetAll",
                    new { paginationDTO.Page, paginationDTO.recordsPerPage },
                    commandType: CommandType.StoredProcedure);

                var moviesCount = await connection.QuerySingleAsync<int>("Movies_Count",
                    commandType: CommandType.StoredProcedure);

                _httpContext.Response.Headers.Append("totalAmountOfRecords", moviesCount.ToString());

                return movies.ToList();
            }
        }

        public async Task<Movie?> GetByIdAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var movie = await connection.QueryFirstOrDefaultAsync<Movie>("Movies_GetById",
                    new { id }, commandType: CommandType.StoredProcedure);

                return movie;
            }
        }
                                                    
        public async Task UpdateAsync(Movie movie)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("Movies_Update",
                    new
                    {
                        movie.Id,
                        movie.Title,
                        movie.InTheaters,
                        movie.ReleaseDate,
                        movie.Poster
                    }, 
                    commandType: CommandType.StoredProcedure);
            }
        }
    }
}