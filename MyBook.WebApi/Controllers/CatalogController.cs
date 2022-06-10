using AuthorizationServer.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBook.DataAccess;
using MyBook.Entity;
using MyBook.WebApi.Services;
using OpenIddict.Validation.AspNetCore;

namespace MyBook.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class CatalogController : Controller
{
    private readonly ApplicationContext _context;
    private readonly AuthorizeManager _auth;

    private readonly UserManager<User> _userManager;
    
    public CatalogController(ApplicationContext context, AuthorizeManager auth, UserManager<User> userManager)
    {
        _userManager = userManager;
        _context = context;
        _auth = auth;
    }

    [HttpGet]
    [Route("GetAllBooks")]
    public async Task<IActionResult> Books() =>
        Ok(await _context.Books.Include(a => a.Author).ToListAsync());

    [HttpGet]
    [Route("GetAllFreeBooks")]
    public async Task<IActionResult> FreeBooks() =>
        Ok(await _context.Books
            .Include(a => a.Author)
            .Where(s => s.SubType == 0)
            .ToListAsync());
    
    [HttpGet]
    [Route("Premium")]
    [AuthorizeViaBearer]
    public async Task<IActionResult> Premium()
    {
        if (!await _auth.HasRole(HttpContext,"UserSub") && !await _auth.HasRole(HttpContext,"Admin"))
        {
            return StatusCode(403);
        }
        
        return Ok(await _context.Books
            .Include(a => a.Author)
            .Where(s => s.SubType == 1)
            .ToListAsync());
    }

    [HttpGet]
    [Route("GetBook/{id}")]
    [AuthorizeViaBearer]
    public async Task<IActionResult> BookDetails(Guid id)
    {
        var book = await _context.Books
            .Include(a => a.Author)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book is null)
            return NotFound(new {Error = "Unexpected id"});
        
        

        if (book.SubType == 1)
        {
            if (!await _auth.HasRole(HttpContext,"UserSub") && !await _auth.HasRole(HttpContext,"Admin"))
            {
                return StatusCode(403);
            }
            
        }

        return Ok(book);
    }
    
    
    [HttpGet]
    [Route("GetAuthor/{id}")]
    public async Task<IActionResult> AuthorDetails(Guid id)
    {
        var author = await _context.Authors
            .Include(a => a.Books)
            .FirstOrDefaultAsync(b => b.Id == id);
    
        if (author is null)
            return NotFound(new {Error = "Unexpected id"});

        return Ok(author);
    }
    
    
    [HttpGet]
    [Route("Search/")]
    public async Task<IActionResult> Search(int selectId, string? keyword)
    {
        if (keyword == null)
            return NotFound(new {Error = "Keyword is null or empty"});

        if (selectId == 1)
        {
            var books = (await _context.Books
                    .Include(a => a.Author)
                    .ToListAsync())
                .Where(x => string.Concat(x.Title.ToLower().Where(c => !char.IsWhiteSpace(c)))
                    .Contains(string.Concat(keyword.ToLower().Where(c => !char.IsWhiteSpace(c)))));
    
            return Ok(books);
        }
    
        if (selectId == 2)
        {
            var authors = (await _context.Authors.ToListAsync())
                .Where(x => string.Concat(x.FullName.ToLower().Where(c => !char.IsWhiteSpace(c)))
                    .Contains(string.Concat(keyword.ToLower().Where(c => !char.IsWhiteSpace(c)))));
    
            return Ok(authors);
        }
    
        return NotFound(new {Error = "selectId is incorrect"});
    }
}