using FeatureFlags.Core.Dtos;
using FeatureFlags.Core.Entities;
using FeatureFlags.Core.Repositories;

namespace FeatureFlags.Core.Services
{
    public interface IPostService
    {
        Task CreatePostAsync(Post post);
        Task DeletePostAsync(int postId);
        Task<PostDto?> GetPostByIdAsync(int postId);
        Task<IEnumerable<PostDto>> LoadPostsAsync(int start, int length);
        Task UpdatePostAsync(Post post);
    }

    internal sealed class PostService(IPostRepository postRepository) : IPostService
    {
        private readonly IPostRepository _postRepository = postRepository ?? throw new ArgumentNullException(nameof(postRepository));

        public async Task<IEnumerable<PostDto>> LoadPostsAsync(int start, int length)
        {
            try
            {
                return await _postRepository.LoadPostsAsync(start, length);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<PostDto?> GetPostByIdAsync(int postId)
        {
            try
            {
                return await _postRepository.GetPostByIdAsync(postId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task CreatePostAsync(Post post)
        {
            try
            {
                await _postRepository.CreatePostAsync(post);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdatePostAsync(Post post)
        {
            try
            {
                await _postRepository.UpdatePostAsync(post);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task DeletePostAsync(int postId)
        {
            try
            {
                await _postRepository.DeletePostAsync(postId);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
