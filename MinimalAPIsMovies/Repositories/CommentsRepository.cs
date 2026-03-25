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

        public CommentsRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
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

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Comment>> GetAllAsync(PaginationDTO paginationDTO)
        {
            throw new NotImplementedException();
        }

        public Task<Comment?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Comment comment)
        {
            throw new NotImplementedException();
        }
    }
}
