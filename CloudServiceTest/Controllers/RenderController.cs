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


		public struct TestVertex
		{
			public Vector3 position;
			public Vector3 normal;
			public Vector2 uv;
		}


		public IActionResult Render()
		{

			// test model data
			TestVertex[] vertices = new TestVertex[24];
			vertices[0].position = new Vector3(-0.5f, 0.5f, -0.5f);
			vertices[1].position = new Vector3(0.5f, 0.5f, -0.5f);
			vertices[2].position = new Vector3(-0.5f, -0.5f, -0.5f);
			vertices[3].position = new Vector3(0.5f, -0.5f, -0.5f);

			vertices[0].normal = new Vector3(0, 0, -1);
			vertices[1].normal = new Vector3(0, 0, -1);
			vertices[2].normal = new Vector3(0, 0, -1);
			vertices[3].normal = new Vector3(0, 0, -1);

			vertices[4].position = new Vector3(-0.5f, 0.5f, 0.5f);
			vertices[5].position = new Vector3(0.5f, 0.5f, 0.5f);
			vertices[6].position = new Vector3(-0.5f, 0.5f, -0.5f);
			vertices[7].position = new Vector3(0.5f, 0.5f, -0.5f);

			vertices[4].normal = new Vector3(0, 1, 0);
			vertices[5].normal = new Vector3(0, 1, 0);
			vertices[6].normal = new Vector3(0, 1, 0);
			vertices[7].normal = new Vector3(0, 1, 0);

			vertices[8].position = new Vector3(-0.5f, 0.5f, 0.5f);
			vertices[9].position = new Vector3(-0.5f, 0.5f, -0.5f);
			vertices[10].position = new Vector3(-0.5f, -0.5f, 0.5f);
			vertices[11].position = new Vector3(-0.5f, -0.5f, -0.5f);

			vertices[8].normal = new Vector3(-1, 0, 0);
			vertices[9].normal = new Vector3(-1, 0, 0);
			vertices[10].normal = new Vector3(-1, 0, 0);
			vertices[11].normal = new Vector3(-1, 0, 0);

			vertices[12].position = new Vector3(0.5f, 0.5f, -0.5f);
			vertices[13].position = new Vector3(0.5f, 0.5f, 0.5f);
			vertices[14].position = new Vector3(0.5f, -0.5f, -0.5f);
			vertices[15].position = new Vector3(0.5f, -0.5f, 0.5f);

			vertices[12].normal = new Vector3(1, 0, 0);
			vertices[13].normal = new Vector3(1, 0, 0);
			vertices[14].normal = new Vector3(1, 0, 0);
			vertices[15].normal = new Vector3(1, 0, 0);

			vertices[16].position = new Vector3(0.5f, 0.5f, 0.5f);
			vertices[17].position = new Vector3(-0.5f, 0.5f, 0.5f);
			vertices[18].position = new Vector3(0.5f, -0.5f, 0.5f);
			vertices[19].position = new Vector3(-0.5f, -0.5f, 0.5f);

			vertices[16].normal = new Vector3(0, 0, 1);
			vertices[17].normal = new Vector3(0, 0, 1);
			vertices[18].normal = new Vector3(0, 0, 1);
			vertices[19].normal = new Vector3(0, 0, 1);

			vertices[20].position = new Vector3(-0.5f, -0.5f, -0.5f);
			vertices[21].position = new Vector3(0.5f, -0.5f, -0.5f);
			vertices[22].position = new Vector3(-0.5f, -0.5f, 0.5f);
			vertices[23].position = new Vector3(0.5f, -0.5f, 0.5f);

			vertices[20].normal = new Vector3(0, -1, 0);
			vertices[21].normal = new Vector3(0, -1, 0);
			vertices[22].normal = new Vector3(0, -1, 0);
			vertices[23].normal = new Vector3(0, -1, 0);

			for (int i = 0; i < vertices.Length; i += 4)
			{
				vertices[i].uv = new Vector2(0, 0);
				vertices[i + 1].uv = new Vector2(1, 0);
				vertices[i + 2].uv = new Vector2(0, 1);
				vertices[i + 3].uv = new Vector2(1, 1);
			}

			int[] indices =
				[
				0, 1, 2, 2, 1, 3,
				4, 5, 6, 6, 5, 7,
				8,9,10,10,9,11,
				12,13,14,14,13,15,
				16,17,18,18,17,19,
				20,21,22,22,21,23
				];

			var data = new
			{
				vertices,
				indices
			};

			var json = JsonConvert.SerializeObject(data);
			ViewData["test"] = json;

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
