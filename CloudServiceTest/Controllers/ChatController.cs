using Microsoft.AspNetCore.Mvc;

namespace CloudServiceTest.Controllers
{
	public class ChatController : Controller
	{
		public async Task<IActionResult> Chat()
		{
			return View();
		}
	}
}
