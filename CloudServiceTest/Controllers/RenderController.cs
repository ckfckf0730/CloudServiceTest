using CloudServiceTest.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Numerics;

namespace CloudServiceTest.Controllers
{
	public class RenderController : Controller
	{
		private readonly FileStorageService _fileStorageService;
		private readonly DatabaseService _databaseService;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly UserManager<ApplicationUser> _userManager;

		public RenderController(FileStorageService fileStorageService, DatabaseService databaseService,
			SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
		{
			_fileStorageService = fileStorageService;
			_databaseService = databaseService;
			_signInManager = signInManager;
			_userManager = userManager;
		}

		public IActionResult Render()
		{
			// test picture data
			if (IsLogin(out var userName))
			{
				var list = _databaseService.LoadFileRecord(userName);
				Random random = new Random();

				if (list.Count == 0)
				{
					ViewData["testPicture"] = null;
					return View();
				}

				int randomIndex = random.Next(0, list.Count);

				var fileRecord = list[randomIndex];

				var stream = _fileStorageService.DownloadFileAsync("sharedfolders", fileRecord.Id.ToString()).Result;
				using (var memoryStream = new MemoryStream())
				{
					stream.CopyTo(memoryStream);
					var imageBytes = memoryStream.ToArray();
					var base64String = Convert.ToBase64String(imageBytes);

					var fileExtension = Path.GetExtension(fileRecord.FileName).ToLower();
					string mimeType = fileExtension switch
					{
						".jpg" or ".jpeg" => "image/jpeg",
						".png" => "image/png",
						".gif" => "image/gif",
						".bmp" => "image/bmp",
						_ => "application/octet-stream"  // 默认 MIME 类型，处理未知的扩展名
					};

					var data2 = $"data:{mimeType};base64,{base64String}";
					ViewData["testPicture"] = data2;

				}

			}

			return View();
		}

		private bool IsLogin(out string userName)
		{
			if (!_signInManager.IsSignedIn(HttpContext.User))
			{
				userName = string.Empty;
				return false;
			}

			userName = _userManager.GetUserName(User) ?? string.Empty;
			return true;
		}
	}
}
