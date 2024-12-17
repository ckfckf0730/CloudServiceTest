using Microsoft.AspNetCore.Mvc;
using CloudServiceTest;
using CloudServiceTest.Models;
using Microsoft.AspNetCore.Identity;
using CloudServiceTest.Models.Database;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CloudServiceTest.Models.Azure;
using Newtonsoft.Json;
using Azure;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class AzureController : Controller
{
    private readonly FileStorageService _fileStorageService;
    private readonly DatabaseService _databaseService;
    private readonly ImageService _imageService;
    private readonly ImageAnalysisService _imageAnalysisService;
    private readonly BingSearchService _bingSearchService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    private readonly string _azureShareFolder = "sharedfolders";

    public AzureController(FileStorageService fileStorageService, DatabaseService databaseService,
        ImageService imageService, ImageAnalysisService imageAnalysisService, BingSearchService bingSearchService,
        SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _fileStorageService = fileStorageService;
        _databaseService = databaseService;
        _imageService = imageService;
        _imageAnalysisService = imageAnalysisService;
        _bingSearchService = bingSearchService;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public IActionResult Upload()
    {
        return View();
    }


    public async Task<IActionResult> PictureList()
    {
        if (!IsLogin(out var userName))
        {
            var errorModel = new CommonResultModel { Message = "Please Login！" };
            return View("CommonResult", errorModel);
        }

        return View("PictureList");
    }

    [HttpGet]
    public async IAsyncEnumerable<string> StreamThumbnailData()
    {
        int maxPic = 24;

        if (IsLogin(out var userName))
        {
            var list = _databaseService.LoadFileRecord(userName);

			Response.ContentType = "application/json";
			using var writer = new StreamWriter(Response.Body);

			for (var i = 0; i < maxPic; i++)
            {
                if (i >= list.Count)
                {
				    break;
                }

                var item = list[i];

                var stream = await _fileStorageService.DownloadFileAsync(_azureShareFolder, item.ThumbnailId.ToString()); 
                if (stream == null)
                {
                    continue;
                }
                string imageSrc = null;
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    var imageBytes = memoryStream.ToArray();
                    var base64String = Convert.ToBase64String(imageBytes);

                    var fileExtension = Path.GetExtension(item.FileName).ToLower();
                    string mimeType = fileExtension switch
                    {
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".png" => "image/png",
                        ".gif" => "image/gif",
                        ".bmp" => "image/bmp",
                        _ => "application/octet-stream"  // 默认 MIME 类型，处理未知的扩展名
                    };

                    imageSrc = $"data:{mimeType};base64,{base64String}";

                }

				var data = new
				{
                    dataType ="img",
					name = item.FileName,
					imageSrc = imageSrc,
					resId = item.Id.ToString(),
					tag = item.Tag
				};


				var json = JsonConvert.SerializeObject(data);
				await writer.WriteAsync(json + "\n");
				await writer.FlushAsync(); // 确保每次都立即发送
            }

            Random random = new Random();
            int randomIndex = random.Next(0, list.Count);
            var tag = list[randomIndex].Tag;
            var bingResponse = await _bingSearchService.SearchAsync(tag);

			var data2 = new
			{
				dataType = "bing",
				webSearchUrl = bingResponse.webSearchUrl,
				name = bingResponse.name,
				thumbnailUrl = bingResponse.thumbnailUrl
			};
			var json2 = JsonConvert.SerializeObject(data2);
			await writer.WriteAsync(json2 + "\n");
			await writer.FlushAsync();

			await Response.BodyWriter.CompleteAsync();
		}

		yield break;
	}

	private Stream? DownLoadFromAzure(string azureName)
    {
        return _fileStorageService.DownloadFileAsync(_azureShareFolder, azureName).Result;
    }

    [HttpGet]
    public IActionResult DownloadAndSave(Guid guid)
    {
        var fileRecord = _databaseService.GetFileRecordAsync(guid).Result;
        var stream = _fileStorageService.DownloadFileAsync(_azureShareFolder, fileRecord.Id.ToString()).Result;
        var extension = Path.GetExtension(fileRecord.FileName).ToLower();
        string mimeType = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            _ => "application/octet-stream"
        };

        return File(stream, mimeType, fileRecord.FileName);
    }

    [HttpGet]
    public IActionResult DisplayImage(Guid guid)
    {
        var fileRecord = _databaseService.GetFileRecordAsync(guid).Result;

        var stream = _fileStorageService.DownloadFileAsync(_azureShareFolder, fileRecord.Id.ToString()).Result;
        var extension = Path.GetExtension(fileRecord.FileName).ToLower();
        string mimeType = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            _ => "application/octet-stream"
        };

        return File(stream, mimeType);
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

    [HttpPost]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        string userName;
        if (!IsLogin(out userName))
        {
            var model = new CommonResultModel { Message = "Please Upload Picture after Login！" };
            return View("CommonResult", model);
        }

        if (file == null || string.IsNullOrEmpty(file.FileName))
        {
            var model = new CommonResultModel { Message = "Please Select a picture！" };
            return View("CommonResult", model);
        }

        if (!_imageService.IsImage(file))
        {
            var model = new CommonResultModel { Message = "Upload file must be a picture！" };
            return View("CommonResult", model);
        }

        var guid = Guid.NewGuid();
        var thumbnailGuid = Guid.NewGuid();

        var newFile = new FileRecord
        {
            Id = Guid.NewGuid(),
            FileName = file.FileName,
            FilePath = file.FileName,
            UploadedBy = userName,
            UploadDate = DateTime.UtcNow,
            ThumbnailId = thumbnailGuid
        };

        using (var transaction = await _databaseService.GetTransactionAsync())
        {
            bool isSuccess = false;
            string ErrorMsg = null;
            try
            {
                using (var stream = file.OpenReadStream())
                {
                    var tag = await _imageAnalysisService.AnalyzeImageAsync(stream);
                    newFile.Tag = tag;
                    var saveResult = await _databaseService.SaveFileRecordAsync(newFile);

                    if (saveResult == 1)
                    {
                        stream.Position = 0;
                        var updateName = newFile.Id.ToString();
                        var result = await _fileStorageService.UploadFileAsync(_azureShareFolder, updateName, stream);
                        var str = result.GetRawResponse();
                        isSuccess = !str.IsError;
                        ErrorMsg = str.ReasonPhrase;
                    }
                }
            }
            catch (Exception ex)
            {
				await transaction.RollbackAsync();
                return Json(new { success = false, message = "File uploaded faultily:" + ex.Message });
            }


            if (isSuccess)
            {
                transaction.Commit();
				await UploadThumbnail(file, newFile.ThumbnailId.ToString());
                return Json(new { success = true, message = "File uploaded successfully." });
            }
            else
            {
				await transaction.RollbackAsync();
                return Json(new { success = false, message = "File upload failed." });
            }
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteFile(Guid guid)
    {
        var record = await _databaseService.GetFileRecordAsync(guid);

        using (var transaction = await _databaseService.GetTransactionAsync())
        {
            var num = await _databaseService.DeleteFileRecordAsync(guid);
            if (num == 1)
            {
                try
                {
                    await _fileStorageService.DeleteFileAsync(_azureShareFolder, record.ThumbnailId.ToString());
                    var result = await _fileStorageService.DeleteFileAsync(_azureShareFolder, guid.ToString());
                    if (!result)
                    {
                        transaction.Rollback();
                        return Content("Delete Azure file Error: " + guid);
                    }
                    else
                    {
                        transaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Content("File delete faultily: " + ex.Message);
                }
            }
            else if (num == 0)
            {
                return Content("File delete faultily. Can't delete datebase row: " + guid);
            }
            else
            {
                transaction.Rollback();
                return Content("File delete faultily: Delete more than 1 detabase rows.");
            }
        }

        return await PictureList();
    }

    private async Task UploadThumbnail(IFormFile file, string name)
    {
        var data = _imageService.GenerateThumbnail(file);
        if (data == null)
        {
            return;
        }

        using (var stream = new MemoryStream(data))
        {
            var result = await _fileStorageService.UploadFileAsync(_azureShareFolder, name, stream);
            var str = result.GetRawResponse();
        }
    }
}

