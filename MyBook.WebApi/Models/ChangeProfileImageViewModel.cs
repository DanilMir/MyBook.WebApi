using System.ComponentModel.DataAnnotations;

namespace MyBook.Models;

public class ChangeProfileImageViewModel
{
    [Required]
    public IFormFile Image { get; set; } = null!;
}