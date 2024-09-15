using Microsoft.AspNetCore.Mvc;
using CloudServiceTest;
using CloudServiceTest.Models;
using Microsoft.AspNetCore.Identity;
using CloudServiceTest.Models.Database;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CloudServiceTest.Models.Azure;

public class AzureController : Controller
{
    private readonly FileStorageService _fileStorageService;
    private readonly DatabaseService _databaseService;
    private readonly ImageService _imageService;
    private readonly ImageAnalysisService _imageAnalysisService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    private readonly string _azureShareFolder = "sharedfolders"; 

    public AzureController(FileStorageService fileStorageService, DatabaseService databaseService,
        ImageService imageService, ImageAnalysisService imageAnalysisService,
        SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _fileStorageService = fileStorageService;
        _databaseService = databaseService;
        _imageService = imageService;
        _imageAnalysisService = imageAnalysisService;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public IActionResult Upload()
    {
        return View();
    }


    public IActionResult PictureList()
    {
        ThumbnailViewModel model = new ThumbnailViewModel();
        model.DataList = new List<ThumbnailData>();

        if (IsLogin(out var userName))
        {
            var list = _databaseService.LoadFileRecord(userName);
            foreach(var item in list)
            {
                var stream = DownLoadFromAzure(item.ThumbnailId.ToString());
                if(stream == null)
                {
                    continue;
                }
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

                    var imageSrc = $"data:{mimeType};base64,{base64String}";

                    ThumbnailData thumb = new ThumbnailData { Name = item.FileName, 
                        ImageSrc = imageSrc , ResId = item.Id};
                    model.DataList.Add(thumb);
                }
            }

            return View(model);
        }

        var errorModel = new CommonResultModel { Message = "Please Login！" };
        return View("CommonResult", errorModel);
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
        if(!IsLogin(out userName))
        {
            var model = new CommonResultModel { Message = "Please Upload Picture after Login！" };
            return View("CommonResult", model);
        }

        if (file == null || string.IsNullOrEmpty(file.FileName))
        {
            var model = new CommonResultModel { Message = "Please Select a picture！" };
            return View("CommonResult", model);
        }

        if(!_imageService.IsImage(file))
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

        using(var transaction = await _databaseService.GetTransactionAsync())
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

                    if(saveResult == 1)
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
                transaction.RollbackAsync();
                return Content("File uploaded faultily:" + ex.Message);
            }


            if (isSuccess)
            {
                transaction.Commit();
                UpdateThumbnail(file, newFile.ThumbnailId.ToString());
                return Content("File uploaded successfully.");
            }
            else
            {
                transaction.RollbackAsync();
                return Content("File uploaded faultily:" + ErrorMsg);
            }
        }
    }

    private async Task UpdateThumbnail(IFormFile file, string name)
    {
        var data = _imageService.GenerateThumbnail(file);
        if(data == null)
        {
            return;
        }

        using(var stream = new MemoryStream(data))
        {
            var result = await _fileStorageService.UploadFileAsync(_azureShareFolder, name, stream);
            var str = result.GetRawResponse();
        }
    }
}

