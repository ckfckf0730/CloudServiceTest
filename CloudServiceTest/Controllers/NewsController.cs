using CloudServiceTest.Models;
using Microsoft.AspNetCore.Mvc;

namespace CloudServiceTest.Controllers
{
    public class NewsController : Controller
    {
        public IActionResult News()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateNews(NewsViewModel model)
        {


            return View("News");
        }
    }
}
