using AuthorizationServer.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyBook.DataAccess;
using MyBook.Entity;
using MyBook.Entity.Identity;
using MyBook.WebApi.Services;

namespace MyBook.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class SubController : Controller
{
    private readonly ApplicationContext _context;

    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;
    private readonly AuthorizeManager _auth;

    public SubController(
        ApplicationContext context, 
        UserManager<User> userManager, 
        RoleManager<Role> roleManager,
        AuthorizeManager auth)
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
        _auth = auth;
    }
    
    [HttpGet]
    [Route("Pay")]
    [AuthorizeViaBearer]
    public async Task<IActionResult> Pay(int subId)
    {
        var curUser = await _userManager.GetUserAsync(HttpContext.User);

        if (subId < 1 || subId > 4)
        {
            return BadRequest($"subId{subId} is not exist");
        }
        
        if (await _auth.HasRole(HttpContext, "UserSub"))
        {
            return Conflict(error: "subscription already purchased");
        }
        
        curUser.SubId = subId;
        curUser.SubDateStart = DateTime.Now;

        await _userManager.UpdateAsync(curUser);
        await _userManager.AddToRoleAsync(curUser, "UserSub");


        return Ok("payment was successful");
    }
    
    [HttpGet]
    [Route("ResetSub")]
    [AuthorizeViaBearer]
    public async Task<IActionResult> ResetSub()
    {
        if (!await _auth.HasRole(HttpContext, "UserSub"))
        {
            return Conflict(error: "nothing to reset");
        }
        
        var curUser = await _userManager.GetUserAsync(HttpContext.User);

        curUser.SubDateStart = default;
        curUser.SubId = 4;
        await _userManager.RemoveFromRoleAsync(curUser, "UserSub");
        
        return Ok("subscription reset successful");
    }
}