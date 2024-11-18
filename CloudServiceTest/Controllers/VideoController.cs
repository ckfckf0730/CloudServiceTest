using CloudServiceTest.Models.Database;
using CloudServiceTest.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using CloudServiceTest.Models.Video;

namespace CloudServiceTest.Controllers
{
	public class VideoController : Controller
	{
		private readonly VideoService _videoService;
		private readonly DatabaseService _databaseService;
		private readonly FileStorageService _fileStorageService;

		private readonly string _videoShareFolder = "videofolders";

		public VideoController(VideoService videoService,
			DatabaseService databaseService, FileStorageService fileStorageService)
		{
			_videoService = videoService;
			_databaseService = databaseService;
			_fileStorageService = fileStorageService;

		}

		public async Task<IActionResult> Video()
		{
			var records = await _databaseService.Context.VideoRecords.
				OrderByDescending(r => r.UploadDate).Take(10).ToListAsync();

			VideoViewModel model = new VideoViewModel();
			model.videos = new List<VideoInfo>();
			foreach(var record in records)
			{
				model.videos.Add(new VideoInfo()
				{
					videoID = record.Id.ToString(),
					videoName = record.FileName,
					videoType = record.VideoType
				});
			}
			return View(model);
		}

		public IActionResult VideoStreaming(string guid, string name, string type)
		{
			VideoInfo videoInfo = new VideoInfo();
			videoInfo.videoName = name;
			videoInfo.videoType = type;
			videoInfo.videoURL = _fileStorageService.GetStreamingVideoURL(_videoShareFolder, guid);
			return View("VideoStreaming", videoInfo);
		}


		[HttpPost]
		public async Task<IActionResult> UploadFile(IFormFile file)
		{
			if (file == null || string.IsNullOrEmpty(file.FileName))
			{
				var model = new CommonResultModel { Message = "Please Select a video！" };
				return View("CommonResult", model);
			}

			if (!_videoService.IsVideo(file))
			{
				var model = new CommonResultModel { Message = "Upload file must be a video！" };
				return View("CommonResult", model);
			}

			var guid = Guid.NewGuid();
			var thumbnailGuid = Guid.NewGuid();

			var newFile = new VideoRecord
			{
				Id = Guid.NewGuid(),
				FileName = file.FileName,
				FilePath = file.FileName,
				UploadedBy = "",
				UploadDate = DateTime.UtcNow,
				Tag = "",
				State = "Pending",
				ChunkCount = 1,
				VideoType = file.ContentType.ToLower()
			};
			var dbContext = _databaseService.Context;

			bool isSuccess = false;
			string ErrorMsg = null;
			try
			{
				await dbContext.VideoRecords.AddAsync(newFile);
				var saveResult = await dbContext.SaveChangesAsync();

				using (var stream = file.OpenReadStream())
				{
					if (saveResult == 1)
					{
						var updateName = newFile.Id.ToString();
						isSuccess = await _fileStorageService.UploadFileAsBlobAsync(_videoShareFolder, updateName, stream);
					}

				}
			}
			catch (Exception ex)
			{
				newFile.State = "Fault";
				dbContext.VideoRecords.Update(newFile);
				await dbContext.SaveChangesAsync();
				return Json(new { success = false, message = "File uploaded faultily:" + ex.Message });
			}


			if (isSuccess)
			{
				newFile.State = "Uploaded";
				dbContext.VideoRecords.Update(newFile);
				await dbContext.SaveChangesAsync();
				return Json(new { success = true, message = "File uploaded successfully." });
			}
			else
			{
				newFile.State = "";
				dbContext.VideoRecords.Update(newFile);
				await dbContext.SaveChangesAsync();
				return Json(new { success = false, message = "File upload failed." });
			}
		}

	}
}
