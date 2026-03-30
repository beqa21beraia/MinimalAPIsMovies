using Dapper;
using Microsoft.Data.SqlClient;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Interfaces;
using System.Data;
using System.Security.AccessControl;

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
                    new { id }, commandType: CommandType.StoredProcedure);
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
                using (var multi = await connection.QueryMultipleAsync("Movies_GetById", new { id }))
                {
                    var movie = await multi.ReadFirstAsync<Movie>();
                    var comments = await multi.ReadAsync<Comment>();

                    movie.Comments = comments.ToList();

                    return movie;
                }
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

        public async Task AssignAsync(int id, List<int> genresIds)
        {
            var dt = new DataTable();
            dt.Columns.Add("Id", typeof(int));

            foreach(int genreId in genresIds)
            {
                dt.Rows.Add(genreId);
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("Movies_AssignGenres",
                    new { movieId = id, genresIds = dt },
                    commandType: CommandType.StoredProcedure);
            }
        }

        public async Task AssignAsync(int id, List<ActorMovie> actors)
        {
            for (int i = 1; i <= actors.Count; i++)
            {
                actors[i - 1].Order = i;
            }

            var dt = new DataTable();
            dt.Columns.Add("ActorId", typeof(int));
            dt.Columns.Add("Character", typeof(string));
            dt.Columns.Add("Order", typeof(int));

            foreach(var actor in actors)
            {
                dt.Rows.Add(actor.ActorId, actor.Character, actor.Order);
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("Movies_AssignActors",
                    new { movieId = id, actors = dt },
                    commandType: CommandType.StoredProcedure);
            }
        }
    }
}