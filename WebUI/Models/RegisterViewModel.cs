using System.ComponentModel.DataAnnotations;

namespace WebUI.Models;

public class RegisterViewModel
{
	[Required(ErrorMessage ="First Name is required")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = String.Empty;
    [Display(Name = "Last Name")]
    [Required(ErrorMessage ="Last Name is required")]
    public string LastName { get; set; } = String.Empty;
    [Required(ErrorMessage ="Email is required")]
    public string Email { get; set; } = String.Empty;
    [Required(ErrorMessage ="Password is required")]
    public string Password { get; set; } = String.Empty;

    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = String.Empty;
    [Display(Name = "Address")]
    public string? Address { get; set; } = String.Empty; 

}
