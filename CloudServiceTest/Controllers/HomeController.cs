using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    // 显示初始页面
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    // 接收文本并返回确认消息
    [HttpPost]
    public IActionResult SubmitMessage(string message)
    {
        return Content($"已收到消息: {message}");
    }
}