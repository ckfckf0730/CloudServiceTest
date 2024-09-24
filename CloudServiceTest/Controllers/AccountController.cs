using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CloudServiceTest.Models;
using Newtonsoft.Json.Linq;
using System.Formats.Asn1;

namespace CloudServiceTest.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CheckConfirmed()
        {
            var user = await _userManager.GetUserAsync(User);
            if(user == null )
            {
                return Content("No User");
            }
            if(await _userManager.IsEmailConfirmedAsync(user))
            {
                return Content("The user has been confirmed.");
            }

            return Content("Not been confirmed.");
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        // create token
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = token }, Request.Scheme);

                    // Send e-mail to comfirm regist
                    await _emailSender.SendEmailAsync(model.Email, "Confirm your email",
                        $"Please confirm your account by clicking this link: <a href='{confirmationLink}'>link</a>");

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    var successModel = new RegisterResultViewModel { Message = "Regist Successfully！ You will receive a confirm e-mail, please check." };
                    return View("RegisterResultViewModel", successModel);//RedirectToAction("Index", "Home");
                }

                var faultModel = new RegisterResultViewModel { Message = "Regist faultily！" };
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    faultModel.Message += "\n" + error.Description;
                }

                return View("RegisterResultViewModel", faultModel);
            }

            return View(model);
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return Content("EmailConfirmed Error");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Content("EmailConfirmed Error");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                await AddRole();
				return Content("EmailConfirmed, you've gotten the <Admin> role");
            }

            return Content("EmailConfirmed Error");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = token }, Request.Scheme);

                        // Send e-mail to comfirm regist
                        await _emailSender.SendEmailAsync(model.Email, "Confirm your email",
                            $"Please confirm your account by clicking this link: <a href='{confirmationLink}'>link</a>");

                        await _signInManager.SignInAsync(user, isPersistent: false);
                        var successModel = new RegisterResultViewModel { Message = "Regist Successfully！ You will receive a confirm e-mail, please check." };
                    }

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        private async Task AddRole()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var isAdmin = await _userManager.IsInRoleAsync(user,"Admin");
                if (!isAdmin)
                {
                    await _userManager.AddToRoleAsync(user, "Admin");
                }
                await _signInManager.SignInAsync(user, isPersistent: false);
            }

        }

        public async Task<IActionResult> DeleteAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Content("Can't find Current User");
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Content("Delete Account Error.");
        }
    }
}
