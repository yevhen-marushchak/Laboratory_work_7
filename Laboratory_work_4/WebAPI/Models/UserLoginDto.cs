using System.ComponentModel.DataAnnotations;

namespace Cafe.WebAPI.Models;

public class UserLoginDto
{
    [Required]
    public string Username { get; set; }
    [Required]
    public string Password { get; set; }
}

public class UserRegisterDto
{
    [Required]
    public string Username { get; set; }
    [Required]
    public string Password { get; set; }
}