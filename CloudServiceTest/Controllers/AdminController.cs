using CloudServiceTest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CloudServiceTest.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(UserManager<ApplicationUser> userManager)
        {
			_userManager = userManager;

		}

        [HttpGet]
        public async Task<ActionResult> Admin()
        {
            int pageNumber = 1; // The page number you want to retrieve
            int pageSize = 20;  // Number of items per page

            var userList = _userManager.Users.Skip((pageNumber - 1) * pageSize)
                                        .Take(pageSize)
                                        .ToList();


            AdminViewModel model = new AdminViewModel();
            model.Users = userList;
            return View(model);
        }
    }
}
