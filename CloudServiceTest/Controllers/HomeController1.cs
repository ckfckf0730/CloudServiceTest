using Microsoft.AspNetCore.Mvc;
using CloudServiceTest;

public class HomeController1 : Controller
{
    private readonly FileStorageService _fileStorageService;

    public IActionResult Index()
    {
        return View();
    }

    public HomeController1(FileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }


    [HttpPost]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        using (var stream = file.OpenReadStream())
        {
            await _fileStorageService.UploadFileAsync("Sharedfolders", file.FileName, stream);
        }

        return Content("File uploaded successfully.");
    }
}

