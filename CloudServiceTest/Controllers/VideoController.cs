using CloudServiceTest.Models.Database;
using CloudServiceTest.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

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

		public IActionResult Video()
		{
			return View();
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
						long chunkSize = 4 * 1024 * 1024; // 4MB per chunk
						long fileLength = stream.Length;
						long chunkCount = (fileLength + chunkSize - 1) / chunkSize;
						newFile.ChunkCount = (int)chunkCount;

						stream.Position = 0;
						isSuccess = true;
						for (int i = 0; i < chunkCount; i++)
						{
							long offset = i * chunkSize;
							long remainingBytes = fileLength - offset;
							int currentChunkSize = (int)Math.Min(chunkSize, remainingBytes);  

							byte[] buffer = new byte[currentChunkSize];
							stream.Seek(offset, SeekOrigin.Begin);  
							await stream.ReadAsync(buffer, 0, currentChunkSize);

							var updateName = newFile.Id.ToString();
							var result = await _fileStorageService.UploadFileChunkAsync(_videoShareFolder, updateName,i, buffer);
							var str = result.GetRawResponse();
							if(str.IsError)
							{
								isSuccess = false;
								ErrorMsg = str.ReasonPhrase;
								break;
							}
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
