using Dapper;
using FeatureFlags.Core.Dtos;
using FeatureFlags.Core.Entities;
using System.Data;

namespace FeatureFlags.Core.Repositories
{
    public interface IUserRepository
    {
        Task CreateUserAsync(User user);
        Task DeleteUserAsync(int userId);
        Task<User?> GetUserByIdAsync(int userId);
        Task<IEnumerable<UserDto>> LoadUsersAsync(int start, int length, int? flag = null);
        Task UpdateUserAsync(User user);
        Task<User?> GetUserByUsernameOrEmailAsync(string username, string email, int userIdToExclude = 0);
    }

    internal sealed class UserRepository(IDbConnection dbConnection) : IUserRepository
    {
        private readonly IDbConnection _dbConnection = dbConnection;

        public async Task<IEnumerable<UserDto>> LoadUsersAsync(int start, int length, int? flag = null)
        {
            string conditionQuery = string.Empty;

            if (flag != null)
            {
                if (flag == 0)
                {
                    conditionQuery += $"AND Flags = 0";
                }
                else
                {
                    conditionQuery += $"AND (Flags & {flag}) = {flag}";
                }
            }

            string query = $@"
SELECT 
    Id,
    Username,
	Email,
	CreatedAt,
	ModifiedAt,
	Flags,
	COUNT(*) OVER() AS DataCount
FROM Users
WHERE 1=1
{conditionQuery}
ORDER BY CreatedAt DESC
";

            if (length > 0)
            {
                query += $" OFFSET {start} ROWS FETCH NEXT {length} ROWS ONLY";
            }

            return await _dbConnection.QueryAsync<UserDto>(query);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<User>("SELECT * FROM Users WHERE Id = @UserId", new { UserId = userId });
        }

        public async Task CreateUserAsync(User user)
        {
            const string insertSql = @"INSERT INTO Users (Username, Email, CreatedAt, ModifiedAt, Flags) 
                               VALUES (@Username, @Email, GETUTCDATE(), GETUTCDATE(), @Flags)";

            if (_dbConnection.State != ConnectionState.Open)
            {
                _dbConnection.Open();
            }

            using var transaction = _dbConnection.BeginTransaction();

            try
            {
                await _dbConnection.ExecuteAsync(insertSql, user, transaction);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task UpdateUserAsync(User user)
        {
            const string sql = @"UPDATE Users SET Username = @Username, Email = @Email, 
                        ModifiedAt = GETUTCDATE(), Flags = @Flags 
                        WHERE Id = @Id";

            if (_dbConnection.State != ConnectionState.Open)
            {
                _dbConnection.Open();
            }

            using var transaction = _dbConnection.BeginTransaction();

            try
            {
                await _dbConnection.ExecuteAsync(sql, user, transaction);
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task DeleteUserAsync(int userId)
        {
            const string sql = @"DELETE FROM Users WHERE Id = @UserId";

            if (_dbConnection.State != ConnectionState.Open)
            {
                _dbConnection.Open();
            }

            using var transaction = _dbConnection.BeginTransaction();

            try
            {
                await _dbConnection.ExecuteAsync(sql, new { UserId = userId }, transaction);
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<User?> GetUserByUsernameOrEmailAsync(string username, string email, int userIdToExclude = 0)
        {
            string conditionQuery = string.Empty;
            object param = new { username, email };

            if (userIdToExclude != 0)
            {
                conditionQuery += "AND Id != @userIdToExclude";
                param = new { username, email, userIdToExclude };
            }

            string query = $@"
SELECT 
* 
FROM Users 
WHERE (Username = @username OR Email = @email) 
{conditionQuery}";

            return await _dbConnection.QueryFirstOrDefaultAsync<User>(query, param);
        }
    }
}
