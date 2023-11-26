using Dapper;
using FeatureFlags.Core.Dtos;
using FeatureFlags.Core.Entities;
using System.Data;
using System.Data.Common;

namespace FeatureFlags.Core.Repositories
{
    public interface IPostRepository
    {
        Task CreatePostAsync(Post post);
        Task DeletePostAsync(int postId);
        Task<PostDto?> GetPostByIdAsync(int postId);
        Task<IEnumerable<PostDto>> LoadPostsAsync(int start, int length, string keyword = "", int userId = 0);
        Task UpdatePostAsync(Post post);
        Task<Post?> GetPostByTitleAndUserIdAsync(string title, int userId, int postId = 0);
        Task<int> GetRandomUserIdAsync();
        Task<bool> TitleExistsAsync(string title);
    }

    internal sealed class PostRepository(IDbConnection dbConnection) : IPostRepository
    {
        private readonly IDbConnection _dbConnection = dbConnection;

        public async Task<IEnumerable<PostDto>> LoadPostsAsync(int start, int length, string keyword = "", int userId = 0)
        {
            var queryParams = new
            {
                start,
                length,
                keyword = string.IsNullOrWhiteSpace(keyword) ? null : $"%{keyword.Trim()}%",
                userId
            };

            List<string> conditions = [];

            if (!string.IsNullOrWhiteSpace(queryParams.keyword))
            {
                conditions.Add("(p.Title LIKE @keyword OR p.Content LIKE @keyword)");
            }

            if (queryParams.userId != 0)
            {
                conditions.Add("u.Id = @userId");
            }

            string conditionQuery = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";

            string query = $@"
SELECT 
    p.Id,
    p.Title,
    p.Content,
    u.UserName,
    p.CreatedAt,
    p.ModifiedAt,
    COUNT(*) OVER() AS DataCount
FROM Posts AS p
JOIN Users AS u ON u.Id = p.UserId
{conditionQuery}
ORDER BY p.CreatedAt DESC
OFFSET @start ROWS FETCH NEXT @length ROWS ONLY";

            return await _dbConnection.QueryAsync<PostDto>(query, queryParams);
        }

        public async Task<PostDto?> GetPostByIdAsync(int postId)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<PostDto>("SELECT * FROM Posts WHERE Id = @PostId", new { PostId = postId });
        }

        public async Task CreatePostAsync(Post post)
        {
            const string insertSql = @"INSERT INTO Posts (Title, Content, UserId, CreatedAt, ModifiedAt) 
                               VALUES (@Title, @Content, @UserId, GETUTCDATE(), GETUTCDATE())";

            if (_dbConnection.State != ConnectionState.Open)
            {
                _dbConnection.Open();
            }

            using var transaction = _dbConnection.BeginTransaction();

            try
            {
                await _dbConnection.ExecuteAsync(insertSql, post, transaction);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task UpdatePostAsync(Post post)
        {
            const string sql = @"UPDATE Posts SET Title = @Title, Content = @Content, 
                        ModifiedAt = GETUTCDATE() 
                        WHERE Id = @Id";

            if (_dbConnection.State != ConnectionState.Open)
            {
                _dbConnection.Open();
            }

            using var transaction = _dbConnection.BeginTransaction();

            try
            {
                await _dbConnection.ExecuteAsync(sql, post, transaction);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task DeletePostAsync(int postId)
        {
            const string sql = @"DELETE FROM Posts WHERE Id = @PostId";

            if (_dbConnection.State != ConnectionState.Open)
            {
                _dbConnection.Open();
            }

            using var transaction = _dbConnection.BeginTransaction();

            try
            {
                await _dbConnection.ExecuteAsync(sql, new { PostId = postId }, transaction);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<Post?> GetPostByTitleAndUserIdAsync(string title, int userId, int postId = 0)
        {
            string conditionQuery = string.Empty;
            object param = new { Title = title, UserId = userId };

            if (postId != 0)
            {
                conditionQuery += "AND Id != @PostId";
                param = new { Title = title, UserId = userId, PostId = postId };
            }

            string sql = $@"
SELECT 
*
FROM Posts
WHERE Title = @Title 
AND UserId = @UserId 
{conditionQuery}";

            return await _dbConnection.QueryFirstOrDefaultAsync<Post>(sql, param);
        }

        public async Task<int> GetRandomUserIdAsync()
        {
            string query = "SELECT TOP 1 Id FROM Users ORDER BY NEWID()";

            return await _dbConnection.QueryFirstOrDefaultAsync<int>(query);
        }

        public async Task<bool> TitleExistsAsync(string title)
        {
            string query = "SELECT COUNT(*) FROM Posts WHERE Title = @Title;";
            var parameters = new { Title = title };

            var count = await _dbConnection.ExecuteScalarAsync<int>(query, parameters);

            return count > 0;
        }
    }
}
