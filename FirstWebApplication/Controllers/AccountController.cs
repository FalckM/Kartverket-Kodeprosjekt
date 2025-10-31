﻿using FirstWebApplication.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
    public class AccountController : Controller
    {
        // UserManager handles user operations (create, find, update user)
        private readonly UserManager<IdentityUser> _userManager;

        // SignInManager handles sign in and sign out operations
        private readonly SignInManager<IdentityUser> _signInManager;

        // Constructor that injects UserManager and SignInManager
        // ASP.NET Core automatically provides these when the controller is created
        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: /Account/AuthPage
        // Shows the combined login/register page (for users who want to create account)
        [HttpGet]
        public IActionResult AuthPage()
        {
            // If user is already logged in, redirect to workspace
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Workspace", "Obstacle");
            }

            return View();
        }

        // POST: /Account/Register
        // Handles registration when user submits the form
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // ModelState.IsValid checks that all validation requirements are met
            if (ModelState.IsValid)
            {
                // Creates a new IdentityUser with the email from the form
                // UserName is set to the same as Email (common practice)
                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email
                };

                // CreateAsync attempts to create the user in the database
                // Password is automatically hashed by Identity
                var result = await _userManager.CreateAsync(user, model.Password);

                // If user creation succeeded
                if (result.Succeeded)
                {
                    // Automatically log in the user after registration
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    //  NEW: Redirect user directly to the workspace! 
                    return RedirectToAction("Workspace", "Obstacle");
                }

                // If something went wrong, add error messages to ModelState
                // This will display error messages in the form
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we get here, validation failed
            // Show the form again with error messages and keep register panel active
            ViewBag.ShowRegister = true;
            return View("AuthPage");
        }

        // POST: /Account/Login
        // Handles login when user submits the form from the landing page
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // PasswordSignInAsync attempts to log in the user
                // - model.Email: the username (in our case email)
                // - model.Password: the password (checked against hashed password in database)
                // - model.RememberMe: whether the user should stay logged in
                // - lockoutOnFailure: false = don't lock account on wrong password
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false
                );

                if (result.Succeeded)
                {
                    //  NEW: Login succeeded! Redirect to workspace instead of home ⭐
                    return RedirectToAction("Workspace", "Obstacle");
                }

                // If login failed, show a general error message
                // We don't say whether it's email or password that's wrong (security)
                ModelState.AddModelError(string.Empty, "Invalid login attempt");
            }

            // Show the landing page again with error messages
            // The landing page (Home/Index) will display the login form again
            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/Logout
        // Logs out the user
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // SignOutAsync removes login information
            await _signInManager.SignOutAsync();

            //  NEW: Redirect user back to the landing page (Home/Index) ⭐
            return RedirectToAction("Index", "Home");
        }
    }
}