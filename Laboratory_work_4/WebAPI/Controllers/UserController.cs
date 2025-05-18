using Cafe.BLL.Facades;
using Cafe.WebAPI.Models;
using Cafe.WebAPI.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Cafe.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly CafeFacade _cafeFacade;
    private readonly IConfiguration _configuration;

    public UserController(CafeFacade cafeFacade, IConfiguration configuration)
    {
        _cafeFacade = cafeFacade;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto model)
    {
        var user = await _cafeFacade.LoginAsync(model.Username, model.Password);
        if (user == null)
            return Unauthorized("Невірний логін або пароль.");

        var token = JwtTokenGenerator.GenerateToken(user.Username, _configuration);

        return Ok(new
        {
            Token = token,
            Username = user.Username
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto model)
    {
        var success = await _cafeFacade.RegisterAsync(model.Username, model.Password);
        if (!success)
            return BadRequest("Користувач із таким іменем уже існує.");

        return Ok("Користувач зареєстрований.");
    }
}