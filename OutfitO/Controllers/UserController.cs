﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OutfitO.Models;
using OutfitO.ViewModels;
using OutfitO.Repository;
using static System.Net.Mime.MediaTypeNames;
using System.Net;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OutfitO.Controllers
{
    public class UserController : Controller
    {
        private UserManager<User> userManager;
        private SignInManager<User> signInManager;
        ICartRepository cartRepository;

        public UserController(UserManager<User> userManager, SignInManager<User> signInManager,ICartRepository cart)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.cartRepository = cart;
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View("Register");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(UserVM uservm, IFormFile ProfileImage)
        {
			if (ProfileImage !=null && ProfileImage.Length > 0)
			{
                string FileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ProfileImage.FileName);
				string path = $"wwwroot/Images/{FileName}";
				FileStream fs = new FileStream(path, FileMode.Create);
				ProfileImage.CopyTo(fs);
				uservm.ProfileImage = FileName;
            }
			if (uservm.FirstName !=null && uservm.Lastname !=null && uservm.Password !=null && uservm.ConfirmPassword !=null && 
                uservm.PhoneNumber !=null  && uservm.Gender !=null && uservm.Address !=null){
                User user = new User
                {
                    FirstName = uservm.FirstName,
                    Lastname = uservm.Lastname,
                    Email = uservm.Email,
                    PasswordHash = uservm.Password,
                    PhoneNumber = uservm.PhoneNumber,
                    Visa = uservm.Visa,
                    ProfileImage = uservm.ProfileImage,
                    Gender = uservm.Gender,
                    Address = uservm.Address,
                    UserName=uservm.FirstName+uservm.Lastname+ Guid.NewGuid().ToString()
                };
                IdentityResult result = await userManager.CreateAsync(user, uservm.Password);
                if (result.Succeeded)
                {
                    IdentityResult resultRole = await userManager.AddToRoleAsync(user,"User");
                    await signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Login", "User");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View("Register",uservm);
            }
            return View("Register",uservm);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM uservm)
        {
            if (ModelState.IsValid==true)
            {
                User data = await userManager.FindByEmailAsync(uservm.Email);
                if(data != null)
                {
                    bool found = await userManager.CheckPasswordAsync(data, uservm.Password);
                    if (found)
                    {
                        await signInManager.SignInAsync(data, uservm.RememberMe);//Id,Name,[Role]
                        HttpContext.Session.SetString("UserName",data.FirstName );
                        HttpContext.Session.SetString("UserId",data.Id );
                        int CartCount = cartRepository.GetForUser(data.Id).Count;
						HttpContext.Session.SetInt32("Count", CartCount);
						HttpContext.Session.SetString("UserImg",data.ProfileImage );
                        //return RedirectToAction("Index", "Product");
                        return RedirectToAction("Index", "Profile");

                    }
                }
                ModelState.AddModelError("", "Invalid Account");
            }
            return View("Login", uservm);
        }

        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "User");
        }
    }
}
