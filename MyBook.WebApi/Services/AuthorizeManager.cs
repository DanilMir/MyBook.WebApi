using Microsoft.AspNetCore.Identity;
using MyBook.DataAccess;
using MyBook.Entity;
using MyBook.Entity.Identity;

namespace MyBook.WebApi.Services;

public class AuthorizeManager
{
    private readonly ApplicationContext _context;

    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;

    public AuthorizeManager(ApplicationContext context, UserManager<User> userManager, RoleManager<Role> roleManager)
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<bool> HasRole(HttpContext httpContext, string role)
    {
        var curUser = await _userManager.GetUserAsync(httpContext.User);
        return await _userManager.IsInRoleAsync(curUser, role);
    }
}