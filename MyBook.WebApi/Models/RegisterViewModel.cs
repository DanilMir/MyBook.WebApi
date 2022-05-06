using System.ComponentModel.DataAnnotations;

namespace MyBook.Models;

public class RegisterViewModel
{
    public string grant_type { get; set; }
    
    public string username { get; set; } = null!;
    
    public string password { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string Lastname { get; set; } = null!;
    
    
    [EmailAddress]
    public string Email { get; set; } = null!;
}