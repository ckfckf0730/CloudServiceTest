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

		private static string s_defaultThumbnailSrc;

		public VideoController(VideoService videoService,
			DatabaseService databaseService, FileStorageService fileStorageService, IWebHostEnvironment hostingEnvironment)
		{
			_videoService = videoService;
			_databaseService = databaseService;
			_fileStorageService = fileStorageService;

			if (s_defaultThumbnailSrc == null)
			{
				var bytes = System.IO.File.ReadAllBytes(hostingEnvironment.WebRootPath + "/resource/texture_cantFindThum.png");
				var base64String = Convert.ToBase64String(bytes);
				string mimeType = "image/png";
				s_defaultThumbnailSrc = $"data:{mimeType};base64,{base64String}";
			}

		}

		public async Task<IActionResult> Video()
		{
			var records = await _databaseService.Context.VideoRecords.
				OrderByDescending(r => r.UploadDate).Take(10).ToListAsync();

			VideoViewModel model = new VideoViewModel();
			model.videos = new List<VideoInfo>();
			foreach (var record in records)
			{
				string tumbSrc = null;
				if (string.IsNullOrEmpty(record.Thumbnail))
				{
					tumbSrc = s_defaultThumbnailSrc;
				}
				else
				{
					var thumbStream = await _fileStorageService.DownloadBlobFileAsync(_videoShareFolder, record.Thumbnail);

					if (thumbStream == null)
					{
						tumbSrc = s_defaultThumbnailSrc;
					}
					else
					{
						using (var memoryStream = new MemoryStream())
						{
							thumbStream.CopyTo(memoryStream);
							var imageBytes = memoryStream.ToArray();
							var base64String = Convert.ToBase64String(imageBytes);

							string mimeType = "application/octet-stream";

							tumbSrc = $"data:{mimeType};base64,{base64String}";

						}
					}
				}

				model.videos.Add(new VideoInfo()
				{
					videoID = record.Id.ToString(),
					videoName = record.FileName,
					videoType = record.VideoType,
					tumbnailSrc = tumbSrc,
				});



			}
			return View("Video", model);
		}

		public IActionResult VideoStreaming(string guid, string name, string type)
		{
			VideoInfo videoInfo = new VideoInfo();
			videoInfo.videoName = name;
			videoInfo.videoType = type;
			videoInfo.videoURL = _fileStorageService.GetStreamingVideoURL(_videoShareFolder, guid);
			return View("VideoStreaming", videoInfo);
		}

		public async Task<IActionResult> DeleteVideo(string guid)
		{
			if (string.IsNullOrWhiteSpace(guid))
			{
				return NoContent();
			}

			if (await _fileStorageService.DeleteFileAsBlobAsync(_videoShareFolder, guid))
			{
				await _databaseService.Context.VideoRecords.Where(fr => fr.Id == Guid.Parse(guid)).ExecuteDeleteAsync();
			}

			return await Video();
		}


		[HttpPost]
		public async Task<IActionResult> UploadFile(IFormFile file, IFormFile thumbnail)
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
			Guid thumbnailGuid = Guid.Empty;
			if (thumbnail != null)
			{
				thumbnailGuid = Guid.NewGuid();
			}


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
				VideoType = file.ContentType.ToLower(),
				Thumbnail = thumbnailGuid == Guid.Empty ? null : thumbnailGuid.ToString()
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

				if (thumbnail != null)
				{
					using (var stream = thumbnail.OpenReadStream())
					{
						if (saveResult == 1)
						{
							var updateName = thumbnailGuid.ToString();
							isSuccess = await _fileStorageService.UploadFileAsBlobAsync(_videoShareFolder, updateName, stream);
						}

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
