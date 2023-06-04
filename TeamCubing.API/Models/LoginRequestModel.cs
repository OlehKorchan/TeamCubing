using System.ComponentModel.DataAnnotations;

namespace TeamCubing.API.Models;

public class LoginRequestModel
{
    [Required(ErrorMessage = "Please, specify valid user login")]
    public string Login { get; set; }

    [Required(ErrorMessage = "Please, specify valid user password")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}
