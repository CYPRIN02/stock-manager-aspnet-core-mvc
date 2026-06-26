// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace StockManager.Web.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;

            _logger.LogDebug(
                "Page login chargée | ReturnUrl={ReturnUrl} | TraceId={TraceId}",
                returnUrl,
                HttpContext.TraceIdentifier);
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!ModelState.IsValid)
            {
                _logger.LogWarning(
                    "Tentative de connexion invalide : modèle non valide | Email={Email} | TraceId={TraceId}",
                    Input?.Email,
                    HttpContext.TraceIdentifier);

                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);

            if (user == null)
            {
                _logger.LogWarning(
                    "Tentative de connexion refusée : email introuvable | Email={Email} | TraceId={TraceId}",
                    Input.Email,
                    HttpContext.TraceIdentifier);

                ModelState.AddModelError(string.Empty, "Identifiant ou mot de passe incorrect.");
                return Page();
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName,
                Input.Password,
                Input.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation(
                    "Utilisateur connecté | UserId={UserId} | Email={Email} | RememberMe={RememberMe} | TraceId={TraceId}",
                    user.Id,
                    user.Email,
                    Input.RememberMe,
                    HttpContext.TraceIdentifier);

                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    _logger.LogDebug("Redirection post-login vers Dashboard | Role=Admin | Email={Email}", user.Email);
                    return RedirectToAction("Index", "Dashboard");
                }

                if (await _userManager.IsInRoleAsync(user, "Manager"))
                {
                    _logger.LogDebug("Redirection post-login vers Dashboard | Role=Manager | Email={Email}", user.Email);
                    return RedirectToAction("Index", "Dashboard");
                }

                if (await _userManager.IsInRoleAsync(user, "Employee"))
                {
                    _logger.LogDebug("Redirection post-login vers Products | Role=Employee | Email={Email}", user.Email);
                    return RedirectToAction("Index", "Products");
                }

                if (await _userManager.IsInRoleAsync(user, "Visitor"))
                {
                    _logger.LogDebug("Redirection post-login vers Home | Role=Visitor | Email={Email}", user.Email);
                    return RedirectToAction("Index", "Home");
                }

                _logger.LogInformation(
                    "Utilisateur connecté sans rôle applicatif connu | Email={Email} | ReturnUrl={ReturnUrl}",
                    user.Email,
                    returnUrl);

                return LocalRedirect(returnUrl);
            }

            if (result.RequiresTwoFactor)
            {
                _logger.LogInformation("Connexion nécessite 2FA | Email={Email}", Input.Email);
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("Compte verrouillé pendant la connexion | Email={Email}", Input.Email);
                return RedirectToPage("./Lockout");
            }

            _logger.LogWarning(
                "Tentative de connexion échouée : mot de passe ou identifiant incorrect | Email={Email} | TraceId={TraceId}",
                Input.Email,
                HttpContext.TraceIdentifier);

            ModelState.AddModelError(string.Empty, "Identifiant ou mot de passe incorrect.");
            return Page();
        }
    }
}
