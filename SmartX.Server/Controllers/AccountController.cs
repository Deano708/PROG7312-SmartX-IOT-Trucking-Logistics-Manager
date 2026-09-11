using Microsoft.AspNetCore.Mvc;

namespace SmartX.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    public record RegisterModel(string CompanyName, string Email, string Password);
    public record LoginModel(string Email, string Password);
    public record ProfileModel(string CompanyName, string Email);

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterModel model)
    {
        // Simple account creation stub
        return Ok(new { Message = "Account registered successfully", Email = model.Email });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel model)
    {
        // Simple authentication check stub
        if (model.Email == "admin@smartx.co.za" && model.Password == "password")
            return Ok(new { Token = "mock-jwt-token", Company = "Smart-X Fleet" });

        return Unauthorized("Invalid credentials.");
    }

    [HttpPut("profile/{email}")]
    public IActionResult UpdateProfile(string email, [FromBody] ProfileModel model)
    {
        return Ok(new { Message = "Profile updated", UpdatedCompany = model.CompanyName });
    }

    [HttpDelete("profile/{email}")]
    public IActionResult DeleteAccount(string email)
    {
        return Ok(new { Message = $"Account {email} deleted successfully." });
    }
}