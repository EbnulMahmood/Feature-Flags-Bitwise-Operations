using FeatureFlags.Core.Dtos;
using FeatureFlags.Core.Entities;
using FeatureFlags.Core.Helpers;
using FeatureFlags.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Web.Controllers
{
    public class PostsController(IPostService postService) : Controller
    {
        private readonly IPostService _postService = postService ?? throw new ArgumentNullException(nameof(postService));

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> PostsDatatable(int draw, int start, int length, CancellationToken token = default)
        {
            List<List<string>> data = [];
            int recordsTotal = 0;
            int recordsFiltered = 0;
            string message = string.Empty;
            bool isSuccess = false;

            try
            {
                length = length <= 0 ? Constants.datatablePageSize : length;

                IEnumerable<PostDto> postList = await _postService.LoadPostsAsync(start, length) ?? [];
                recordsTotal = postList.FirstOrDefault()?.DataCount ?? 0;
                recordsFiltered = recordsTotal;

                int sl = 1 + start;
                foreach (var item in postList)
                {
                    var postActions = GetPostActions(item.Id, item.Title);

                    List<string> row = [
                        (sl++).ToString(),
                        item.Title,
                        item.Content,
                        item.UserId.ToString(),
                        item.CreatedAt.ToString("MMM dd, yyyy hh:mm:ss tt"),
                        item.ModifiedAt?.ToString("MMM dd, yyyy hh:mm:ss tt") ?? "-",
                        postActions
                    ];
                    data.Add(row);
                }

                isSuccess = true;
            }
            catch (OperationCanceledException ex)
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

        private string GetPostActions(int postId, string title)
        {
            return $@"
<div class='btn-group action-links' role='group'>
    <a href='{Url.Action(nameof(Edit), "Posts", new { id = postId })}' class='btn btn-outline-warning action-link'>Edit</a>
    <button type='button' href='#' data-title='{title}' data-id='{postId}' class='btn btn-outline-danger action-link delete-action'>Remove</button>
</div>";
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Post post)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _postService.CreatePostAsync(post);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Failed to create the post.");
                }
            }
            return View(post);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var post = await _postService.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Post post)
        {
            if (id != post.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _postService.UpdatePostAsync(post);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Failed to update the post.");
                }
            }
            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _postService.DeletePostAsync(id);
                return Json(new { isSuccess = true, message = "Post successfully deleted" });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
    }
}
