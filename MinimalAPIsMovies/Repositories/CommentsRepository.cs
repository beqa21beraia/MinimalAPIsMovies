using Dapper;
using Microsoft.Data.SqlClient;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Interfaces;
using System.Data;

namespace MinimalAPIsMovies.Repositories
{
    public class CommentsRepository : ICommentsRepository
    {
        private readonly string _connectionString;
        private readonly HttpContext _httpContext;

        public CommentsRepository(IConfiguration configuration, 
            IHttpContextAccessor httpContextAccessor)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _httpContext = httpContextAccessor.HttpContext!;
        }

        public async Task<int> CreateAsync(Comment comment)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var id = await connection.QuerySingleAsync<int>("Comments_Create",
                    new { comment.Body, comment.MovieId }, commandType: CommandType.StoredProcedure);
                comment.Id = id;

                return id;
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("Comments_Delete",
                    new { id }, commandType: CommandType.StoredProcedure);
            }
        }
        
        public async Task<bool> ExistsAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var exists = await connection.QuerySingleAsync<bool>("Comments_Exists",
                    new { id }, commandType: CommandType.StoredProcedure);

                return exists;
            }
        }

        public async Task<List<Comment>> GetAllAsync(int movieId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var comments = await connection.QueryAsync<Comment>("Comments_GetAllByMovieId",
                    new { movieId }, commandType: CommandType.StoredProcedure);

                return comments.ToList();
            }
        }

        public async Task<Comment?> GetByIdAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var comment = await connection.QueryFirstOrDefaultAsync<Comment>("Comments_GetById",
                    new { id }, commandType: CommandType.StoredProcedure);

                return comment;
            }
        }

        public async Task UpdateAsync(Comment comment)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("Comments_Update",
                    new { comment.Id, comment.Body, comment.MovieId },
                    commandType: CommandType.StoredProcedure);
            }
        }
    }
}
