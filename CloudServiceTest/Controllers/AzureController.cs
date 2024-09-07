using Microsoft.AspNetCore.Mvc;
using CloudServiceTest;
using CloudServiceTest.Models;
using Microsoft.AspNetCore.Identity;
using CloudServiceTest.Models.Database;

public class AzureController : Controller
{
    private readonly FileStorageService _fileStorageService;
    private readonly DatabaseService _databaseService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public IActionResult Upload()
    {
        return View();
    }


    public IActionResult PictureList()
    {
        if(IsLogin(out var userName))
        {
            var list = _databaseService.LoadFileRecord(userName);

            var fileNameList = list.Select(item => item.FileName).ToList();

            return View(fileNameList);
        }

        var model = new CommonResultModel { Message = "Please Upload Picture after Login！" };
        return View("CommonResult", model);
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

    public AzureController(FileStorageService fileStorageService, DatabaseService databaseService,
        SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _fileStorageService = fileStorageService;
        _databaseService = databaseService;
        _signInManager = signInManager;
        _userManager = userManager;
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

        if(!IsImage(file))
        {
            var model = new CommonResultModel { Message = "Upload file must be a picture！" };
            return View("CommonResult", model);
        }

        var guid = Guid.NewGuid();

        var newFile = new FileRecord
        {
            Id = Guid.NewGuid(),
            FileName = file.FileName,
            FilePath = file.FileName,
            UploadedBy = userName,
            UploadDate = DateTime.UtcNow
        };

        _databaseService.SaveFileRecord(newFile);

        bool isSuccess = false;
        using (var stream = file.OpenReadStream())
        {
            stream.Position = 0;
            var updateName = newFile.Id.ToString();
            var result = await _fileStorageService.UploadFileAsync("sharedfolders", updateName, stream);
            var str = result.GetRawResponse();
            isSuccess = !str.IsError; 
        }

        if(isSuccess)
        {
            return Content("File uploaded successfully.");
        }
        else
        {
            return Content("File uploaded faultily.");
        }

    }

    private readonly List<string> AllowedImageMimeTypes = new List<string>
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/bmp"
    };

    private bool IsImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return false;
        }

        // Check MIME type
        if (!AllowedImageMimeTypes.Contains(file.ContentType.ToLower()))
        {
            return false;
        }

        // Optionally: check file header (magic numbers) for further security
        using (var stream = file.OpenReadStream())
        {
            try
            {
                byte[] header = new byte[4];
                stream.Read(header, 0, 4);
                return IsImageHeader(header);
            }
            catch
            {
                return false;
            }
        }
    }

    private static bool IsImageHeader(byte[] header)
    {
        // JPEG
        if (header[0] == 0xFF && header[1] == 0xD8)
            return true;

        // PNG
        if (header[0] == 0x89 && header[1] == 0x50)
            return true;

        //// GIF
        //if (header[0] == 0x47 && header[1] == 0x49)
        //    return true;

        // BMP
        if (header[0] == 0x42 && header[1] == 0x4D)
            return true;

        return false;
    }
}

