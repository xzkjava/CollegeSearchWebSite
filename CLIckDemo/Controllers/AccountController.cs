using CLickDemo.DAL;
using CLickDemo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/*
 * This controller handles interactions with Users in the database.
 */
namespace CLickDemo.Controllers
{
   [Authorize]
    public class AccountController : Controller
    {
        //inject UserManager
        private readonly UserManager<ApplicationUser> _securityManager;
        //inject SignInManager
        private readonly SignInManager<ApplicationUser> _loginManager;


        //inject dependency via constructor
        public AccountController(UserManager<ApplicationUser> secMgr, SignInManager<ApplicationUser> loginManager)
        {
            _securityManager = secMgr;
            _loginManager = loginManager;

        }

        public ActionResult Index()
        {

            return RedirectToAction(nameof(HomeController.ManageFavColleges), "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        //Get: Register page for new user
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Register model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email
                };
                var result = await _securityManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _loginManager.SignInAsync(user, isPersistent: false);
                    //return RedirectToAction(nameof(HomeController.ManageFavColleges), "Home");
                    return RedirectToAction("Login");
                }
                else
                {
                    foreach(IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(model);
        }

        //Get: Login Page
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewBag.returnUrl = returnUrl;
            return View();
        }

        //Post: Log in 
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Login model, string returnUrl = null)
        {
            //ViewBag.returnUrl = returnUrl;
            if (ModelState.IsValid)
            {
                ApplicationUser user = await _securityManager.FindByEmailAsync(model.Email);
                if(user != null)
                {
                    await _loginManager.SignOutAsync();
                    var result = await _loginManager.PasswordSignInAsync(model.Email, model.Password, false, lockoutOnFailure: false);
                    if(result.Succeeded)
                    {
                        return RedirectToReturnUrl(returnUrl);
                    }
                }
                ModelState.AddModelError(nameof(model.Email), "Invalid user or password");
                
            }
            return View(model);
        }
        
      
        //Get: Logout
        public async Task<ActionResult> Logout()
        {
            if (User.Identity.IsAuthenticated)
            {
               await _loginManager.SignOutAsync();
            }
            
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        private ActionResult RedirectToReturnUrl(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

    }
}

