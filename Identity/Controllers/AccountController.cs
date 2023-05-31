using Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace Identity.Controllers
{
	[Authorize]
	public class AccountController : Controller
	{
		private readonly UserManager<IdentityUser> _userManager;
		private readonly SignInManager<IdentityUser> _signInManager;
		private readonly IEmailSender _emailSender;
		private readonly RoleManager<IdentityRole> _roleManager;

		public AccountController( UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IEmailSender emailSender, RoleManager<IdentityRole> roleManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_emailSender = emailSender;
			_roleManager = roleManager;
		}
		public IActionResult Index()
		{
			return View();
		}

		#region Register
		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> Register(string? returnurl = null)
		{

			if(! await _roleManager.RoleExistsAsync("Admin"))
			{
				//Create Roles
				await _roleManager.CreateAsync(new IdentityRole("Admin"));
				await _roleManager.CreateAsync(new IdentityRole("User"));
			}

			List<SelectListItem> listItems = new List<SelectListItem>();

			listItems.Add(new SelectListItem
			{
				Value = "Admin",
				Text = "Admin"
			});
			listItems.Add(new SelectListItem
			{
				Value = "User",
				Text = "User"
			});

			ViewData["ReturnUrl"] = returnurl;
			RegisterViewModel registerViewModel = new RegisterViewModel()
			{
				RoleList = listItems,
			};
			
			return View(registerViewModel);
		}

		[HttpPost]
		[AllowAnonymous]    
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterViewModel model, string? returnurl = null)
		{
			ViewData["ReturnUrl"] = returnurl;
			returnurl = returnurl ?? Url.Content("~/");
			if (ModelState.IsValid)
			{
				

				var user = new ApplicationUser
				{
					FirstName = model.FirstName,
					LastName = model.LastName,
					UserName = model.Email,
					Email = model.Email,
					Password = model.Password
				};

				var result = await _userManager.CreateAsync(user, model.Password);

				

				if (result.Succeeded)
				{
                    if (model.RoleSelected != null && model.RoleSelected.Length > 0 && model.RoleSelected == "Admin")
                    {
                        await _userManager.AddToRoleAsync(user, "Admin");
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, "User");
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
					return LocalRedirect(returnurl);
				}
				AddErrors(result);
			}

            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem
            {
                Value = "Admin",
                Text = "Admin"
            });
            listItems.Add(new SelectListItem
            {
                Value = "User",
                Text = "User"
            });

			model.RoleList = listItems;

            return View(model);
		}
		#endregion

		#region Login
		[HttpGet]
		[AllowAnonymous]

		public IActionResult Login(string? returnurl = null)
		{
			ViewData["ReturnUrl"] = returnurl;
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[AllowAnonymous]

		public async Task<IActionResult> Login(LoginViewModel model, string? returnurl = null)
		{
			ViewData["ReturnUrl"] = returnurl;
			returnurl = returnurl ?? Url.Content("~/");
			if (ModelState.IsValid)
			{
				var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
				if (result.Succeeded)
				{
					return LocalRedirect(returnurl);
				}
				if (result.IsLockedOut)
				{
					return View("Lockout");
				}
				else
				{
					ModelState.AddModelError(string.Empty, "Invalid Login attempt!");
				}
			}

			return View(model);
		}
		#endregion

		#region Logout
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> LogOff()
		{
			await _signInManager.SignOutAsync();

			return RedirectToAction(nameof(HomeController.Index), "Home");
		}
		#endregion

		#region ForgetPassword
		[AllowAnonymous]
		public IActionResult ForgetPassword()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[AllowAnonymous]
		public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel model)
		{
			var user = await _userManager.FindByEmailAsync(model.Email);

			if (ModelState.IsValid)
			{
				if (user == null)
				{
					return RedirectToAction("ForgetPasswordConfirmation");
				}

				var code = await _userManager.GeneratePasswordResetTokenAsync(user);
				var callbackurl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);

				await _emailSender.SendEmailAsync(model.Email, "Reset Password - Identity",
					"Please reset your password by clicking here: <a href=\"" + callbackurl + "\">Link</a>");

				return RedirectToAction("ForgetPasswordConfirmation");
			}
			

			return View();
		}

		[HttpGet]
		[AllowAnonymous] 
		public IActionResult ForgetPasswordConfirmation()
		{
			return View();
		}

		#endregion

		#region Reset Password
		[AllowAnonymous]
		public IActionResult ResetPassword(string code = null)
		{
			return code == null ? View("Error") : View();
		}

		[HttpPost]
		[AllowAnonymous]

		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
		{
			var user = await _userManager.FindByEmailAsync(model.Email);

			if (ModelState.IsValid)
			{
				if (user == null)
				{
					return RedirectToAction("ResetPasswordConfirmation");
				}


				var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);

				if (result.Succeeded)
				{
					return RedirectToAction("ResetPasswordConfirmation");
				}

				AddErrors(result);
			}


			return View();
		}

		[HttpGet]
		[AllowAnonymous]

		public IActionResult ResetPasswordConfirmation()
		{
			return View();
		}

		#endregion

		#region ExternalLogin
		[HttpPost]
		[ValidateAntiForgeryToken]
		[AllowAnonymous]
		public IActionResult ExternalLogin(string provider, string returnurl = null)
		{
			//request a redirect to the external login provider
			var redirectUrl = Url.Action("ExternalLoginCallback","Account", new {RedirectUrl = returnurl });
			var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
			return Challenge(properties, provider);
		}

		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> ExternalLoginCallback(string returnurl = null, string remoteError = null)
		{
			if(remoteError != null)
			{
				ModelState.AddModelError(string.Empty, $"Error from external login provide: {remoteError}");
				return View(nameof(Login));
			}
			var info = await _signInManager.GetExternalLoginInfoAsync();
			if (info == null)
			{
				return RedirectToAction(nameof(Login));
			}
			//signin the user with external login provider, if the user already has a login
			var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
			if(result.Succeeded)
			{
				//update any authentication Token
				await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
				return LocalRedirect(returnurl);
			}
			else
			{
				//if the user does not have an account, then we will ask the user to create an account;
				ViewData["ReturnUrl"] = returnurl;
				ViewData["ProviderDisplayName"] = info.ProviderDisplayName;
				var email = info.Principal.FindFirstValue(ClaimTypes.Email);
				var name = info.Principal.FindFirstValue(ClaimTypes.Name);
				return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel 
				{ 
					Email = email,
					Name = name,
				});
			}

			
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[AllowAnonymous]
		public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnurl = null)
		{
			returnurl = returnurl ?? Url.Content("~/");

			if(ModelState.IsValid) 
			{
				//Get the info of the user by the external login provide
				var info = await _signInManager.GetExternalLoginInfoAsync();
				if (info == null)
				{
					return View("Error");
				}

				var user = new ApplicationUser 
				{ 
					UserName = model.Email, 
					Email = model.Email ,
					FirstName = model.Name,
				};

				var result = await _userManager.CreateAsync(user);

				if (result.Succeeded)
				{
					await _signInManager.SignInAsync(user, isPersistent: false);
					await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
					return LocalRedirect(returnurl);
				}
				AddErrors(result);
			}

			ViewData["ReturnUrl"] = returnurl;
			return View(model);	
		}
		#endregion

		#region Errors
		private void AddErrors(IdentityResult result)
		{
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}
		}
		#endregion

	}
}
