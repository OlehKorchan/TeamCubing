using System.ComponentModel.DataAnnotations;

namespace TeamCubing.API.Models;

public class RegisterRequestModel
{
    [Required(ErrorMessage = "Please, specify a valid username")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "Please, specify a valid password")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required(ErrorMessage = "Please, repeat your password")]
    [Compare(nameof(Password), ErrorMessage = "Passwords doesn't match")]
    [DataType(DataType.Password)]
    public string PasswordConfirm { get; set; }
}
