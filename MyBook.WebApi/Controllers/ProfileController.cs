using AuthorizationServer.Web.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBook.DataAccess;
using MyBook.Entity;
using MyBook.Entity.Identity;
using MyBook.Models;
using MyBook.WebApi.Services;

namespace MyBook.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
[AuthorizeViaBearer]
public class ProfileController : Controller
{
    private readonly ApplicationContext _context;

    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;
    private readonly AuthorizeManager _auth;
    private readonly SignInManager<User> _signInManager;

    public ProfileController(
        ApplicationContext context,
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        AuthorizeManager auth,
        SignInManager<User> signInManager)
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
        _auth = auth;
        _signInManager = signInManager;
    }

    [HttpGet]
    [Route("GetCurrentUser")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _userManager.FindByNameAsync(User.Identity?.Name);
        if (user == null)
            return NotFound();

        var sub = (await _context.Subs.FirstOrDefaultAsync(x => x.Id == user.SubId))!;

        var model = new EditProfileViewModel
        {
            Email = user.Email, Name = user.Name, LastName = user.LastName, Image = user.Image,
            Sub = sub
        };
        return Ok(model);
    }


    [HttpPost]
    [Route("ChangePassword")]
    public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        var errors = new List<string>();
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByNameAsync(User.Identity!.Name);
            if (user != null)
            {
                //ToDo: пароль такой же ли?

                var result =
                    await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

                if (result.Succeeded)
                {
                    return Ok("password changed successfully");
                }
                else
                    foreach (var error in result.Errors)
                    {
                        errors.Add(error.Description);
                    }
            }
            else
                errors.Add("user is not found");
        }

        return BadRequest(errors);
    }

    [HttpPost]
    [Route("ChangeImage")]
    public async Task<ActionResult> ChangeImage([FromForm] ChangeProfileImageViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByNameAsync(User.Identity?.Name);
            if (user != null)
            {
                byte[] imageData;
                using (var binaryReader = new BinaryReader(model.Image.OpenReadStream()))
                    imageData = binaryReader.ReadBytes((int) model.Image.Length);

                user.Image = Convert.ToBase64String(imageData);

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return Ok("updated profile photo");
                }
                else
                    return BadRequest("user is not found");
            }
        }

        return BadRequest();
    }

    [HttpPost]
    [Route("ResetImage")]
    public async Task<ActionResult> ResetImage()
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByNameAsync(User.Identity?.Name);
            if (user != null)
            {
                user.Image = Convert.ToBase64String(await System.IO.File.ReadAllBytesAsync("wwwroot/img/user.png"));

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return Ok("profile photo reset");
                }
                else
                    return BadRequest("user is not found");
            }
        }

        return BadRequest();
    }


    [HttpPost]
    [Route("EditProfile")]
    //todo: remove sub, image fields
    public async Task<IActionResult> EditProfile([FromForm] EditProfileViewModel model)
    {
        ModelState.Remove("Image");
        ModelState.Remove("Sub");
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByNameAsync(User.Identity?.Name);
            if (user != null)
            {
                var sub = (await _context.Subs.FirstOrDefaultAsync(x => x.Id == user.SubId))!;
                var oldEmail = user.Email;

                user.Email = model.Email;
                user.UserName = model.Email;
                user.Name = model.Name;
                user.LastName = model.LastName;

                //roflan
                model.Image = user.Image;
                model.Sub = sub;
                //model.SubDurationLeft = sub.Duration - DateTime.Now.Subtract(user.SubDateStart).Days;

                var result = await _userManager.UpdateAsync(user);

                if (oldEmail != user.Email)
                {
                    await _signInManager.SignOutAsync();
                    return Ok();
                    // return RedirectToAction("Index", "Home");
                }

                if (result.Succeeded)
                {
                    return Ok("changes saved!");
                }
                else
                    return BadRequest("user is not found");
            }
        }

        return BadRequest();
    }


    [HttpPost]
    [Route("AddToFavorites")]
    public async Task<IActionResult> AddToFavorites(Guid id)
    {
        var book = await _context.Books.FirstOrDefaultAsync(x => x.Id == id);
        var user = await _context.Users.Include(x => x.FavoriteBooks)
            .FirstOrDefaultAsync(x => x.UserName == User.Identity!.Name);

        if (book is null)
        {
            return BadRequest("book is not exist");
        }

        if (user is not null && !user.FavoriteBooks.Contains(book))
        {
            user.FavoriteBooks.Add(book);
            await _context.SaveChangesAsync();
            return Ok("book added to favorites");
        }

        return NoContent();
    }

    [HttpPost]
    [Route("RemoveFromFavorites")]
    public async Task<IActionResult> RemoveFromFavorites(Guid id)
    {
        var book = await _context.Books.FirstOrDefaultAsync(x => x.Id == id);
        var user = await _context.Users.Include(x => x.FavoriteBooks)
            .FirstOrDefaultAsync(x => x.UserName == User.Identity!.Name);

        if (book is null)
        {
            return BadRequest("book is not exist");
        }

        if (!user!.FavoriteBooks.Contains(book!))
        {
            return BadRequest("book is not in favorites");
        }
        
        user.FavoriteBooks.Remove(book!);
        await _context.SaveChangesAsync();


        return Ok("book removes from favorites");
    }
    
    
    [HttpGet]
    [Route("Favorites")]
    public async Task<IActionResult> Favorites()
    {
        var user = await _context.Users.Include(x => x.FavoriteBooks).ThenInclude(x=> x.Author)
            .FirstOrDefaultAsync(x => x.UserName == User.Identity!.Name);

        if (user is null)
        {
            return BadRequest("user is not found");
        }

        var result = user.FavoriteBooks.Select(book => new
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author.FullName,
            Description = book.Description,
            Genre = book.Genre,
            SubType = book.SubType,
            Image = book.Image,
            Year = book.Year,
            Rating = book.Rating,
            AddedDate = book.AddedDate
        });
        
        return Ok(result);
    }
}