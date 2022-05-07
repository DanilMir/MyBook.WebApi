using AuthorizationServer.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyBook.DataAccess;
using MyBook.Entity;
using MyBook.Entity.Identity;

namespace MyBook.WebApi.Controllers;

public class SubController : Controller
{
    private readonly ApplicationContext _context;

    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;

    public SubController(ApplicationContext context, UserManager<User> userManager, RoleManager<Role> roleManager)
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
    }
    
    [HttpGet]
    [Route("Pay")]
    [AuthorizeViaBearer]
    //todo: если роль уже есть не позволять покупать
    public async Task<IActionResult> Pay(int subId)
    {
        
        //переделать проверку роли на отдельную функцию
        
        var curUser = await _userManager.GetUserAsync(HttpContext.User);

        curUser.SubId = subId;
        curUser.SubDateStart = DateTime.Now;

        await _userManager.UpdateAsync(curUser);
        await _userManager.AddToRoleAsync(curUser, "UserSub");


        return Ok("Оплата прошла успешно!");
    }
    
    [HttpGet]
    [Route("ResetSub")]
    [AuthorizeViaBearer]
    public async Task<IActionResult> ResetSub()
    {
        
        var curUser = await _userManager.GetUserAsync(HttpContext.User);

        curUser.SubDateStart = default;
        curUser.SubId = 4;
        await _userManager.RemoveFromRoleAsync(curUser, "UserSub");
        
        return Ok("Сброс подписки выполнен успешно");
    }
}