using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyBook.Entity;
using MyBook.Models;
using NETCore.MailKit.Core;

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
    
    [HttpPost] 
    [Route("Login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await _signInManager.
                PasswordSignInAsync(model.Email, model.Password, true, false);
            
            if (result.Succeeded)
            {
                return Ok(new {token = ""});
            }
            else
            {
                return NotFound(new {Error = "Неправильный логин и (или) пароль"});
                ModelState.AddModelError("", "Неправильный логин и (или) пароль");
            }
        }
        return NotFound(new {Error = "Неправильный логин и (или) пароль"});
    }
}