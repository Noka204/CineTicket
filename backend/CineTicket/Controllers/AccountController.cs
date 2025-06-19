using AutoMapper;
using CineTicket.DTOs.Auth;
using CineTicket.Models;
using CineTicket.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    private readonly IConfiguration _config;

    public AccountController(IUserService userService, IMapper mapper, IConfiguration config)
    {
        _userService = userService;
        _mapper = mapper;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        var user = _mapper.Map<ApplicationUser>(request);
        var result = await _userService.RegisterAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { status = false, message = string.Join("; ", result.Errors.Select(e => e.Description)) });

        var createdUser = await _userService.GetByUserNameAsync(request.UserName);
        var token = GenerateJwtToken(createdUser!);
        return Ok(new { status = true, message = "Đăng ký thành công", token });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserRequest request)
    {
        var user = await _userService.GetByUserNameAsync(request.UserName);
        if (user == null || !await _userService.CheckPasswordAsync(user, request.Password))
            return Unauthorized(new { status = false, message = "Sai thông tin đăng nhập" });

        var token = GenerateJwtToken(user);
        return Ok(new { status = true, message = "Đăng nhập thành công", token });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var user = await _userService.GetByEmailAsync(request.Email);
        if (user == null) return NotFound();
        var token = await _userService.GeneratePasswordResetTokenAsync(user);
        return Ok(new { status = true, token });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var user = await _userService.GetByEmailAsync(request.Email);
        if (user == null) return NotFound();
        var success = await _userService.ResetPasswordAsync(user, request.Token, request.NewPassword);
        return success ? Ok(new { status = true }) : BadRequest(new { status = false });
    }

    [HttpPut("update-info")]
    public async Task<IActionResult> UpdateInfo([FromBody] UpdateUserInfoRequest request)
    {
        var user = await _userService.GetByIdAsync(request.Id);
        if (user == null) return NotFound();
        user.FullName = request.FullName;
        user.Address = request.Address;
        user.PhoneNumber = request.PhoneNumber;
        var success = await _userService.UpdateUserInfoAsync(user);
        return success ? Ok(new { status = true }) : BadRequest(new { status = false });
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
        new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
        new Claim("FullName", user.FullName ?? string.Empty),
        new Claim("PhoneNumber", user.PhoneNumber ?? string.Empty),
        new Claim("Address", user.Address ?? string.Empty)
    };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddDays(7);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}
