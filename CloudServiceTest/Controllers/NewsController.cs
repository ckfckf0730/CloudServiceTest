using CloudServiceTest.Models;
using CloudServiceTest.Models.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [HttpGet]
		public async Task<IActionResult> NewsContent(Guid newsId)
        {
            var newsRecourd =  await _databaseService.Context.NewsRecords.FirstOrDefaultAsync(n => n.Id == newsId);
            if(newsRecourd == null)
            {
                return Content("Can't find newsId: "+ newsId);
            }

			NewsData model = new NewsData();
			model.Title = newsRecourd.Title;
            model.Content = newsRecourd.Content;

			
			model.Publisher = (await _userManager.FindByIdAsync(newsRecourd.PublisherId.ToString()))?.UserName;

            return View(model);
		}


	}
}
