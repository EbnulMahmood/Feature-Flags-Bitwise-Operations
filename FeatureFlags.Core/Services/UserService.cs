using FeatureFlags.Core.Dtos;
using FeatureFlags.Core.Entities;
using FeatureFlags.Core.Repositories;

namespace FeatureFlags.Core.Services
{
    public interface IUserService
    {
        Task CreateUserAsync(User user);
        Task DeleteUserAsync(int userId);
        Task<User?> GetUserByIdAsync(int userId);
        Task<IEnumerable<UserDto>> LoadUsersAsync(int start, int length, int? flag = null, CancellationToken token = default);
        Task UpdateUserAsync(User user);
    }

    internal sealed class UserService(IUserRepository userRepository) : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository;

        public async Task<IEnumerable<UserDto>> LoadUsersAsync(int start, int length, int? flag = null, CancellationToken token = default)
        {
            try
            {
                if (token.IsCancellationRequested == true)
                {
                    throw new OperationCanceledException(token);
                }

                if (length < 0)
                {
                    throw new InvalidDataException("Page Size is less than zero");
                }

                return await _userRepository.LoadUsersAsync(start, length, flag, token);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            try
            {
                return await _userRepository.GetUserByIdAsync(userId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task CreateUserAsync(User user)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(user.Username))
                {
                    throw new ArgumentException("Username cannot be empty.");
                }

                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    throw new ArgumentException("Email cannot be empty.");
                }

                if (!IsValidEmail(user.Email))
                {
                    throw new ArgumentException("Invalid email format.");
                }

                var existingUser = await UserExists(user.Username, user.Email);
                if (existingUser != false)
                {
                    throw new ArgumentException("User with the same Username or Email already exists.");
                }

                await _userRepository.CreateUserAsync(user);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<bool> UserExists(string username, string email)
        {
            var existingUser = await _userRepository.GetUserByUsernameOrEmailAsync(username, email);
            return existingUser != null;
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public async Task UpdateUserAsync(User user)
        {
            try
            {
                await _userRepository.UpdateUserAsync(user);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task DeleteUserAsync(int userId)
        {
            try
            {
                await _userRepository.DeleteUserAsync(userId);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
