using CloudServiceTest.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CloudServiceTest.Controllers
{
	public class ChatController : Controller
	{
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Chat()
		{
            int pageNumber = 1; // The page number you want to retrieve
            int pageSize = 10;  // Number of items per page

            var userList = _userManager.Users.Skip((pageNumber - 1) * pageSize)
                                        .Take(pageSize)
                                        .ToList();
			var user = await _userManager.GetUserAsync(User);

			UsersViewModel model = new UsersViewModel();
            model.Users = userList;
            model.SelfUser = user;

			return View(model);
		}
	}
}
