using FeatureFlags.Core.Dtos;
using FeatureFlags.Core.Entities;
using FeatureFlags.Core.Helper;
using FeatureFlags.Core.Helpers;
using FeatureFlags.Core.Services;
using FeatureFlags.Core.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace FeatureFlags.Web.Controllers
{
    public class UsersController(IUserService userService) : Controller
    {
        private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UsersDatatable(int draw, int start, int length, int? flag = null, CancellationToken token = default)
        {
            var data = new List<List<string>>();
            int recordsTotal = 0;
            int recordsFiltered = 0;
            string message = string.Empty;
            bool isSuccess = false;

            try
            {
                length = length <= 0 ? Constants.datatablePageSize : length;

                var userList = await _userService.LoadUsersAsync(start, length, flag, token) ?? new List<UserDto>();
                recordsTotal = userList.FirstOrDefault()?.DataCount ?? 0;
                recordsFiltered = recordsTotal;

                int sl = 1 + start;
                foreach (var item in userList)
                {
                    var flags = GetFlags(item.Flags);
                    var userActions = GetUserActions(item.Id);

                    var row = new List<string>
                    {
                        (sl++).ToString(),
                        item.Username,
                        item.Email,
                        item.CreatedAt.ToString("MMM dd, yyyy hh:mm:ss tt"),
                        item.ModifiedAt?.ToString("MMM dd, yyyy hh:mm:ss tt") ?? "-",
                        flags,
                        userActions
                    };
                    data.Add(row);
                }

                isSuccess = true;
            }
            catch(OperationCanceledException ex)
            {
                message = ex.Message;
            }
            catch (InvalidDataException ex)
            {
                message = ex.Message;
            }
            catch (Exception)
            {
                message = "Internal Server Error";
            }

            return Json(new { draw, recordsTotal, recordsFiltered, data, isSuccess, message });
        }

        private static string GetFlags(int? Flags)
        {
            var individualFlags = UserFlagsHelper.GetIndividualFlags(Flags);
            var flagContainer = new StringBuilder("<div class='flag-container'>");

            foreach (var flag in individualFlags)
            {
                flagContainer.Append($"<span class='flag {flag.ToLower().Replace(" ", "-")}'>{flag}</span>");
            }

            flagContainer.Append("</div>");
            return flagContainer.ToString();
        }

        private static string GetUserActions(int userId)
        {
            return $@"
<div class='btn-group action-links' role='group'>
    <a href='/Details/{userId}' class='btn btn-outline-primary action-link'>Details</a>
    <a href='/Edit/{userId}' class='btn btn-outline-secondary action-link'>Edit</a>
    <a href='/Delete/{userId}' class='btn btn-outline-danger action-link delete'>Delete</a>
</div>";
        }


        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound();

                return View(user);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public IActionResult Create()
        {
            var userViewModel = new UserViewModel
            {
                Username = string.Empty,
                Email = string.Empty
            };

            return View(userViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel userViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(userViewModel);
            }

            try
            {
                var combinedFlags = UserFlagsHelper.GetCombinedFlags(userViewModel.Flags);

                var user = new User
                {
                    Username = userViewModel.Username,
                    Email = userViewModel.Email,
                    Flags = combinedFlags
                };

                await _userService.CreateUserAsync(user);
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentNullException ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Failed to create the user.");
            }

            return View(userViewModel);
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound();

                return View(user);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id, Username, Email, CreatedAt, ModifiedAt, Flags")] User user)
        {
            if (id != user.Id)
                return BadRequest("User ID in the request body doesn't match the route parameter.");

            if (ModelState.IsValid)
            {
                try
                {
                    await _userService.UpdateUserAsync(user);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Failed to update the user.");
                }
            }
            return View(user);
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound();

                return View(user);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _userService.DeleteUserAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(Delete), new { id, error = true });
            }
        }
    }
}
