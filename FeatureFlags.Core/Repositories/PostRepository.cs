using Dapper;
using FeatureFlags.Core.Dtos;
using FeatureFlags.Core.Entities;
using System.Data;

namespace FeatureFlags.Core.Repositories
{
    public interface IPostRepository
    {
        Task CreatePostAsync(Post post);
        Task DeletePostAsync(int postId);
        Task<PostDto?> GetPostByIdAsync(int postId);
        Task<IEnumerable<PostDto>> LoadPostsAsync(int start, int length);
        Task UpdatePostAsync(Post post);
    }

    internal sealed class PostRepository(IDbConnection dbConnection) : IPostRepository
    {
        private readonly IDbConnection _dbConnection = dbConnection;

        public async Task<IEnumerable<PostDto>> LoadPostsAsync(int start, int length)
        {
            string query = $@"
SELECT 
    Id,
    Title,
    Content,
    UserId,
    CreatedAt,
    ModifiedAt
FROM Posts
ORDER BY CreatedAt DESC
OFFSET {start} ROWS FETCH NEXT {length} ROWS ONLY";

            return await _dbConnection.QueryAsync<PostDto>(query);
        }

        public async Task<PostDto?> GetPostByIdAsync(int postId)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<PostDto>("SELECT * FROM Posts WHERE Id = @PostId", new { PostId = postId });
        }

        public async Task CreatePostAsync(Post post)
        {
            const string insertSql = @"INSERT INTO Posts (Title, Content, UserId, CreatedAt, ModifiedAt) 
                               VALUES (@Title, @Content, @UserId, GETUTCDATE(), GETUTCDATE())";

            await _dbConnection.ExecuteAsync(insertSql, post);
        }

        public async Task UpdatePostAsync(Post post)
        {
            const string sql = @"UPDATE Posts SET Title = @Title, Content = @Content, 
                        ModifiedAt = GETUTCDATE() 
                        WHERE Id = @Id";

            await _dbConnection.ExecuteAsync(sql, post);
        }

        public async Task DeletePostAsync(int postId)
        {
            const string sql = @"DELETE FROM Posts WHERE Id = @PostId";

            await _dbConnection.ExecuteAsync(sql, new { PostId = postId });
        }
    }
}
