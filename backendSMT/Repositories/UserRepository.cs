using Dapper;
using System.Data.SqlClient;
using backendSMT.Models;

namespace backendSMT.Repositories
{
    public class UserRepository
    {
        private readonly string _connectionString;
        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<User?> GetUserAsync(string username, string password)
        {
            using var conn = new SqlConnection(_connectionString);
            string sql = @"SELECT userId, username, is_employee AS IsEmployee, is_engineer AS IsEngineer
                           FROM users
                           WHERE username = @username AND password = @password";
            return await conn.QueryFirstOrDefaultAsync<User>(sql, new { username, password });
        }
    }
}
