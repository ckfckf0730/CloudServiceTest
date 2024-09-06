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


    public IActionResult Index()
    {
        return View();
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
        if (!_signInManager.IsSignedIn(HttpContext.User))
        {
            var model = new CommonResultModel { Message = "Please Upload Picture after Login！" };
            return View("CommonResult", model);
        }

        if (file == null || string.IsNullOrEmpty(file.FileName))
        {
            var model = new CommonResultModel { Message = "Please Select a picture！" };
            return View("CommonResult", model);
        }

        var guid = Guid.NewGuid();
        var userName = _userManager.GetUserName(User);

        var newFile = new FileRecord
        {
            Id = Guid.NewGuid(),
            FileName = file.FileName,
            FilePath = file.FileName,
            UploadedBy = userName,
            UploadDate = DateTime.UtcNow
        };

        _databaseService.SaveFileRecord(newFile);

        using (var stream = file.OpenReadStream())
        {
            var updateName = newFile.Id.ToString();
            await _fileStorageService.UploadFileAsync("sharedfolders", updateName, stream);
        }

        return Content("File uploaded successfully.");
    }
}

