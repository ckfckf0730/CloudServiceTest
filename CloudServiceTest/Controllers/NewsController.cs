using CloudServiceTest.Models;
using CloudServiceTest.Models.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CloudServiceTest.Controllers
{
    public class NewsController : Controller
    {
		private readonly DatabaseService _databaseService;
		private readonly UserManager<ApplicationUser> _userManager;

		public NewsController(DatabaseService databaseService, UserManager<ApplicationUser> userManager)
        {
            _databaseService = databaseService;
			_userManager = userManager;
		}


		public IActionResult News()
        {
            NewsViewModel model = new NewsViewModel();
			model.NewsArr = _databaseService.Context.NewsRecords.OrderByDescending(n => n.PublishedDate).Take(10).ToArray();


			return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNews(NewsViewModel model)
        {
            if (model == null)
            {
                return Content("CreateNews Error: NewsViewModel is null.");
			}

            var userId = Guid.Parse(_userManager.GetUserId(User));

            NewsRecord record = new NewsRecord()
            {
                Id = Guid.NewGuid(),
                Title = model.NewNews.Title,
                Content = model.NewNews.Content,
                PublisherId = userId,
                PublishedDate = DateTime.Now,
			};

            await _databaseService.Context.NewsRecords.AddAsync(record);
			await _databaseService.Context.SaveChangesAsync();


			return View("News");
        }
    }
}
