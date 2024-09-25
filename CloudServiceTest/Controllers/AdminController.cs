using CloudServiceTest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CloudServiceTest.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager)
        {
			_userManager = userManager;
            _roleManager = roleManager;

        }

        [HttpGet]
        public async Task<ActionResult> Admin()
        {
            int pageNumber = 1; // The page number you want to retrieve
            int pageSize = 10;  // Number of items per page

            var userList = _userManager.Users.Skip((pageNumber - 1) * pageSize)
                                        .Take(pageSize)
                                        .ToList();


            AdminViewModel model = new AdminViewModel();
            model.Users = userList;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Json(new { message = "Can't find user: " + userId });
            }
            var roles = _userManager.GetRolesAsync(user).Result;
            return Json(roles); 
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleManager.Roles.ToListAsync() ;

            var json = Json(roles);
			return json;
        }

		public async Task<IActionResult> AddUserRole(string userName, string roleName)
		{
			var user = await _userManager.FindByNameAsync(userName);
			if (user == null)
			{
				return Json(new { message = "Can't find user: " + userName });
			}

			var isRole = await _userManager.IsInRoleAsync(user, roleName);
			if (!isRole)
			{
				var result = await _userManager.AddToRoleAsync(user, roleName);
                return Json(new { isSucceeded = result.Succeeded });
			}
			
            return Json(new { isSucceeded = false });
		}

	}
}
