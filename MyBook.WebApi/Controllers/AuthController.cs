using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyBook.Entity;
using MyBook.Models;
using NETCore.MailKit.Core;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants.Permissions;
using static OpenIddict.Server.AspNetCore.OpenIddictServerAspNetCoreConstants;

namespace MyBook.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class AuthController : Controller
{
    private readonly UserManager<User> _userManager;

    private readonly SignInManager<User> _signInManager;

    // private readonly IEmailService _emailService;
    public AuthController(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        // _emailService = emailService;
    }

    [HttpPost("Login")]
    [Produces("application/json")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Login([FromForm] AuthorizationData authorizationData)
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        if (request?.IsPasswordGrantType() == true)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                var properties = new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                    [Properties.ErrorDescription] =
                        "The username/password couple is invalid."
                });

                return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            // Validate the username/password parameters and ensure the account is not locked out.
            var result = await _signInManager
                .CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                var properties = new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                    [Properties.ErrorDescription] =
                        "The username/password couple is invalid."
                });

                return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }


            // Create a new ClaimsPrincipal containing the claims that
            // will be used to create an id_token, a token or a code.
            var principal = await _signInManager.CreateUserPrincipalAsync(user);


            // Set the list of scopes granted to the client application.
            principal.SetScopes(new[]
            {
                Scopes.Email,
                Scopes.Profile,
                Scopes.Roles
            }.Intersect(request.GetScopes()));

            foreach (var claim in principal.Claims)
            {
                claim.SetDestinations(GetDestinations(claim, principal));
            }

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new NotImplementedException("The specified grant type is not implemented.");
    }

    [HttpPost("SignUp")]
    [Produces("application/json")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> SignUp([FromForm] RegisterViewModel model)
    {
        var request = HttpContext.GetOpenIddictServerRequest();

        if (request?.IsPasswordGrantType() == true)
        {
            var user = new User
            {
                SubId = 4,
                SubDateStart = default(DateTime),
                Email = model.Email,
                UserName = model.username,
                Name = model.username,
                LastName = model.Lastname,
                Image = Convert.ToBase64String(await System.IO.File.ReadAllBytesAsync("wwwroot/img/user.png")),
                EmailConfirmed = false,
                LockoutEnabled = false
            };

            var result = await _userManager.CreateAsync(user, model.password);

            if (result.Succeeded)
            {
                //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                await _userManager.AddToRoleAsync(user, "User");

                //var link = Url.Action(nameof(VerifyEmail), "Auth", new { userId = user.Id, code },Request.Scheme,Request.Host.ToString());

                //var message = new Message(new string[] { model.Email }, "Подтверждение почты", $"<h2>Добро пожаловать на MyBook!</h2><br><p>Пожалуйста, подтвердите свою почту, перейдя по ссылке</p><a href='{link}'>Подтвердить регистрацию</a>");

                //await _emailService.SendEmailAsync(message);

                //return RedirectToAction("EmailVerification");
                
                var principal = await _signInManager.CreateUserPrincipalAsync(user);

                principal.SetScopes(new[]
                {
                    Scopes.Email,
                    Scopes.Profile,
                    Scopes.Roles
                }.Intersect(request.GetScopes()));

                foreach (var claim in principal.Claims)
                {
                    claim.SetDestinations(GetDestinations(claim, principal));
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }
            else
            {
                var errors = new List<string>();
                foreach (var error in result.Errors)
                {
                    errors.Add(error.Description);
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return BadRequest(
                    new
                    {
                        Message = "Error while registration",
                        Errors = errors
                    });
            }
        }

        var properties = new AuthenticationProperties(new Dictionary<string, string?>
        {
            [Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
            [Properties.ErrorDescription] =
                "Unable to create new user"
        });

        return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }


    private IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal)
    {
        // Note: by default, claims are NOT automatically included in the access and identity tokens.
        // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
        // whether they should be included in access tokens, in identity tokens or in both.

        switch (claim.Type)
        {
            case OpenIddictConstants.Claims.Name:
                yield return OpenIddictConstants.Destinations.AccessToken;

                if (principal.HasScope(Scopes.Profile))
                    yield return OpenIddictConstants.Destinations.IdentityToken;

                yield break;

            case OpenIddictConstants.Claims.Email:
                yield return OpenIddictConstants.Destinations.AccessToken;

                if (principal.HasScope(Scopes.Email))
                    yield return OpenIddictConstants.Destinations.IdentityToken;

                yield break;

            case OpenIddictConstants.Claims.Role:
                yield return OpenIddictConstants.Destinations.AccessToken;

                if (principal.HasScope(Scopes.Roles))
                    yield return OpenIddictConstants.Destinations.IdentityToken;

                yield break;

            // Never include the security stamp in the access and identity tokens, as it's a secret value.
            case "AspNet.Identity.SecurityStamp": yield break;

            default:
                yield return OpenIddictConstants.Destinations.AccessToken;
                yield break;
        }
    }
}