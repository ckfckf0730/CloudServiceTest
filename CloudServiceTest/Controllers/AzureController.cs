using Microsoft.AspNetCore.Mvc;
using CloudServiceTest;
using CloudServiceTest.Models;
using Microsoft.AspNetCore.Identity;
using CloudServiceTest.Models.Database;

public class AzureController : Controller
{
    private readonly FileStorageService _fileStorageService;
    private readonly DatabaseService _databaseService;
    private readonly ImageService _imageService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    private readonly string _azureShareFolder = "sharedfolders"; 

    public AzureController(FileStorageService fileStorageService, DatabaseService databaseService,
        ImageService imageService,
        SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _fileStorageService = fileStorageService;
        _databaseService = databaseService;
        _imageService = imageService;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public IActionResult Upload()
    {
        return View();
    }


    public IActionResult PictureList()
    {
        if(IsLogin(out var userName))
        {
            var list = _databaseService.LoadFileRecord(userName);

            foreach(var item in list)
            {
                DownLoad(item.ThumbnailId.ToString());
            }

            var fileNameList = list.Select(item => item.FileName).ToList();

            return View(fileNameList);
        }

        var model = new CommonResultModel { Message = "Please Upload Picture after Login！" };
        return View("CommonResult", model);
    }

    private void DownLoad(string azureName)
    {
        var stream = _fileStorageService.DownloadFileAsync(_azureShareFolder, azureName).Result;
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
            _databaseService.SaveFileRecord(newFile);
            bool isSuccess = false;
            string ErrorMsg = null;
            try
            {
                using (var stream = file.OpenReadStream())
                {
                    stream.Position = 0;
                    var updateName = newFile.Id.ToString();
                    var result = await _fileStorageService.UploadFileAsync(_azureShareFolder, updateName, stream);
                    var str = result.GetRawResponse();
                    isSuccess = !str.IsError;
                    ErrorMsg = str.ReasonPhrase;
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

