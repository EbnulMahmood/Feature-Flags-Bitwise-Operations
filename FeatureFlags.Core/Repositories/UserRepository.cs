using Dapper;
using FeatureFlags.Core.Dtos;
using FeatureFlags.Core.Entities;
using FeatureFlags.Core.Enums;
using System.Data;

namespace FeatureFlags.Core.Repositories
{
    public interface IUserRepository
    {
        Task CreateUserAsync(User user);
        Task DeleteUserAsync(int userId);
        Task<UserDto?> GetUserByIdAsync(int userId);
        Task<IEnumerable<UserDto>> LoadUsersAsync(int start, int length, int? flag = null, long? viewsMin = null, long? viewsMax = null);
        Task<(IEnumerable<UserDropdownDto>?, bool)> ListUserDropdownAsync(string name, int page, int resultCount);
        Task UpdateUserAsync(User user);
        Task<User?> GetUserByUsernameOrEmailAsync(string username, string email, int userIdToExclude = 0);
    }

    internal sealed class UserRepository(IDbConnection dbConnection) : IUserRepository
    {
        private readonly IDbConnection _dbConnection = dbConnection;

        public async Task<IEnumerable<UserDto>> LoadUsersAsync(int start, int length, int? flag = null, long? viewsMin = null, long? viewsMax = null)
        {
            string conditionQuery = string.Empty;
            string postSubQuery = string.Empty;
            var parameters = new DynamicParameters();

            if (flag != null)
            {
                if (flag == (int)UserFlags.None)
                {
                    conditionQuery += $" AND u.Flags = {(int)UserFlags.None} {Environment.NewLine}";
                }
                else
                {
                    conditionQuery += $" AND (u.Flags & @{nameof(flag)}) = @{nameof(flag)} {Environment.NewLine}";
                    parameters.Add($"@{nameof(flag)}", flag, dbType: DbType.Int32);
                }
            }

            if (viewsMin.HasValue || viewsMax.HasValue)
            {
                if (viewsMin.HasValue)
                {
                    postSubQuery += $"AND p.Views >= @{nameof(viewsMin)} ";
                    parameters.Add($"@{nameof(viewsMin)}", viewsMin, dbType: DbType.Int64);
                }

                if (viewsMax.HasValue)
                {
                    postSubQuery += $"AND p.Views <= @{nameof(viewsMax)} ";
                    parameters.Add($"@{nameof(viewsMax)}", viewsMax, dbType: DbType.Int64);
                }

                postSubQuery = $@"
AND EXISTS (
    SELECT 
        UserId 
    FROM Posts AS p 
    WHERE 1=1 
    {postSubQuery}
    AND u.Id = p.UserId
)
";
            }

            string query = $@"
SELECT 
    u.Id,
    u.Username,
	u.Email,
	u.CreatedAt,
	u.ModifiedAt,
	u.Flags,
	COUNT(*) OVER() AS DataCount
FROM Users AS u
WHERE 1=1
{conditionQuery}
{postSubQuery}
ORDER BY u.CreatedAt DESC
OFFSET @start ROWS FETCH NEXT @length ROWS ONLY
";
            parameters.Add($"@{nameof(start)}", start, dbType: DbType.Int32);
            parameters.Add($"@{nameof(length)}", length, dbType: DbType.Int32);

            return await _dbConnection.QueryAsync<UserDto>(query, parameters);
        }

        public async Task<(IEnumerable<UserDropdownDto>?, bool)> ListUserDropdownAsync(string name, int page, int resultCount)
        {
            int offset = (page - 1) * resultCount;

            string conditionQuery = string.Empty;
            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(name))
            {
                conditionQuery += $" AND Username LIKE @{nameof(name)} {Environment.NewLine}";
                parameters.Add($"@{nameof(name)}", $"%{name.Trim()}%", dbType: DbType.String);
            }

            parameters.Add($"@{nameof(offset)}", offset, dbType: DbType.Int32);
            parameters.Add($"@{nameof(resultCount)}", resultCount, dbType: DbType.Int32);

            string sql = $@"
SELECT
    Id,
    Username AS Text,
    COUNT(*) OVER() AS DataCount
FROM Users
WHERE 1=1
{conditionQuery}
ORDER BY CreatedAt DESC
OFFSET @offset ROWS FETCH NEXT @resultCount ROWS ONLY
";
            var UserDropdownDtoList = await _dbConnection.QueryAsync<UserDropdownDto>(sql, parameters);

            int endCount = offset + resultCount;
            bool morePages = endCount < UserDropdownDtoList?.FirstOrDefault()?.DataCount;

            return (UserDropdownDtoList, morePages);
        }

        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<UserDto>("SELECT * FROM Users WHERE Id = @UserId", new { UserId = userId });
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
            var parameters = new DynamicParameters();

            if (userIdToExclude != 0)
            {
                conditionQuery += $" AND Id != @{nameof(userIdToExclude)} {Environment.NewLine}";
                parameters.Add($"@{nameof(userIdToExclude)}", userIdToExclude, dbType: DbType.Int32);
            }

            parameters.Add($"@{nameof(username)}", username , dbType: DbType.String);
            parameters.Add($"@{nameof(email)}", email, dbType: DbType.String);

            string query = $@"
SELECT 
* 
FROM Users 
WHERE (Username = @{nameof(username)} OR Email = @{nameof(email)}) 
{conditionQuery}";

            return await _dbConnection.QueryFirstOrDefaultAsync<User>(query, parameters);
        }
    }
}
