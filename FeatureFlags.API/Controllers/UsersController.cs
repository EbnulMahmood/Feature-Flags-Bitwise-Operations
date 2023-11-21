using FeatureFlags.Core.Entity;
using FeatureFlags.Core.Service;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> LoadUsers()
        {
            try
            {
                var users = await _userService.LoadUsersAsync();
                return Ok(users);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while fetching users.");
            }
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<User>> GetUserById(int userId)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                    return NotFound();

                return Ok(user);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while fetching the user.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            try
            {
                await _userService.CreateUserAsync(user);
                return CreatedAtAction(nameof(GetUserById), new { userId = user.Id }, user);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while creating the user.");
            }
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] User user)
        {
            try
            {
                if (userId != user.Id)
                    return BadRequest("User ID in the request body doesn't match the route parameter.");

                await _userService.UpdateUserAsync(user);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while updating the user.");
            }
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {
                await _userService.DeleteUserAsync(userId);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting the user.");
            }
        }
    }
}
