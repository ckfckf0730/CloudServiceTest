using Microsoft.AspNetCore.Mvc;
using CloudServiceTest.Sources;

namespace CloudServiceTest.Controllers
{
	public class AIController : Controller
	{
		public IActionResult AIPage()
		{
			AIService.Test();


			return View();
		}
	}
}
