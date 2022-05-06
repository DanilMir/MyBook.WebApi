using System.ComponentModel.DataAnnotations;

namespace MyBook.Models;

public class ChangePasswordViewModel
{
    [Required]
    public string NewPassword { get; set; } = null!;
    [Required]
    public string OldPassword { get; set; } = null!;
}