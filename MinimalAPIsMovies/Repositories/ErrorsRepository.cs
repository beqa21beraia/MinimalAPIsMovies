using Dapper;
using Microsoft.Data.SqlClient;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Interfaces;
using System.Data;

namespace MinimalAPIsMovies.Repositories
{
    public class ErrorsRepository : IErrorsRepository
    {
        private readonly string _connectionString;

        public ErrorsRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<Guid> CreateAsync(Error error)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                error.Id = Guid.NewGuid();
                await connection.ExecuteAsync("Errors_Create",
                    new { error.Id, error.ErrorMessage, error.StackTrace, error.Date },
                    commandType: CommandType.StoredProcedure);

                return error.Id;
            }
        }
    }
}
