﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CloudServiceTest.Models;
using Newtonsoft.Json.Linq;

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
                        // 生成邮箱确认令牌
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = token }, Request.Scheme);

                    // 发送电子邮件包含确认链接
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
                return Content("EmailConfirmed");
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
    }
}
