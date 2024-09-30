using CloudServiceTest.Data;
using CloudServiceTest.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudServiceTest.Controllers
{
	public class ChatController : Controller
	{
        private readonly UserManager<ApplicationUser> _userManager;
		private readonly ApplicationDbContext _applicationDbContext;

		public ChatController(UserManager<ApplicationUser> userManager, ApplicationDbContext applicationDbContext)
        {
            _userManager = userManager;
			_applicationDbContext = applicationDbContext;

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

			ViewBag.DisableChatBox = true;

			return View(model);
		}

		[HttpGet]
		public async Task<IActionResult> GetUserIdByName(string userName)
        {
			var user = await _userManager.FindByNameAsync(userName);
            return Ok(new { userId = user?.Id });
		}

		[HttpGet]
		public async Task<IActionResult> GetMessageById(string messageId)
		{
			var record = await _applicationDbContext.MessageRecords.FindAsync(Guid.Parse(messageId));
			if(record == null)
			{
				return null;
			}

			var sender = await _userManager.FindByIdAsync(record.SenderId.ToString());
			var receiver = await _userManager.FindByIdAsync(record.ReceiverId.ToString());

			return Ok(new
			{
				senderId = record.SenderId,
				receiverId = record.ReceiverId,
				senderName = sender?.UserName,
				receiverName = receiver?.UserName,
				content = record.Content,
				dateTime = record.Timestamp,

			});

		}

		[HttpGet]
		public async Task<IActionResult> GetMessagesOfTwoUser(string selfId, string oppositeUserId)
		{
			var selfGuid = Guid.Parse(selfId);
			var oppositeGuid = Guid.Parse(oppositeUserId);
			var list = await _applicationDbContext.MessageRecords.Where(record =>
			(record.SenderId == selfGuid || record.SenderId == oppositeGuid) &&
			(record.ReceiverId == selfGuid || record.ReceiverId == oppositeGuid)
			).OrderBy(record => record.Timestamp).ToListAsync();

			var response = list.Select(record => new
			{
				senderId = record.SenderId,
				receiverId = record.ReceiverId,
				content = record.Content,
				dateTime = record.Timestamp,
			}).ToArray();

			return Ok(response);
		}
	}
}
