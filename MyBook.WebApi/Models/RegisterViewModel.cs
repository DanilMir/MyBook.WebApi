using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MyBook.Models;

public class RegisterViewModel
{
    [DefaultValue("password")]
    public string grant_type { get; set; }
    
    public string username { get; set; } = null!;
    
    public string password { get; set; } = null!;

    public string name { get; set; } = null!;
    public string lastname { get; set; } = null!;
    
    
    [EmailAddress]
    public string email { get; set; } = null!;
}