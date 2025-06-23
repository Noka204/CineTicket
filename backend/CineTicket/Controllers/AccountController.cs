using AutoMapper;
using CineTicket.DTOs.Accout;
using CineTicket.DTOs.Auth;
using CineTicket.Helpers;
using CineTicket.Models;
using CineTicket.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtTokenGenerator _jwtTokenGenerator;

    public AccountController(IUserService userService, IMapper mapper, IConfiguration config, UserManager<ApplicationUser> userManager, JwtTokenGenerator jwtTokenGenerator)
    {
        _userService = userService;
        _mapper = mapper;
        _config = config;
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        var user = _mapper.Map<ApplicationUser>(request);
        user.Role = SD.Role_Customer;
        var result = await _userService.RegisterAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { status = false, message = string.Join("; ", result.Errors.Select(e => e.Description)) });

        await _userService.AssignRoleAsync(user, SD.Role_Customer);

        var createdUser = await _userService.GetByUserNameAsync(request.UserName);
        return Ok(new { status = true, message = "Đăng ký thành công" });
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserRequest request)
    {
        var user = await _userService.GetByUserNameAsync(request.UserName);
        if (user == null || !await _userService.CheckPasswordAsync(user, request.Password))
            return Unauthorized(new { status = false, message = "Sai thông tin đăng nhập" });

        var token = _jwtTokenGenerator.GenerateToken(user);
        return Ok(new { status = true, message = "Đăng nhập thành công", token });
    }
    [Authorize(Roles = "Employee,Admin")]
    [HttpGet("get-all-users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        var grouped = users
            .GroupBy(u => u.Role)
            .ToDictionary(g => g.Key, g => g.Select(u => new {
                u.FullName,
                u.Email,
                u.UserName,
                u.Role
            }));

        return Ok(new { status = true, data = grouped });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("update-role")]
    public async Task<IActionResult> UpdateUserRole([FromBody] UpdateRoleDto dto)
    {
        var user = await _userService.GetByUserNameAsync(dto.UserName);
        if (user == null)
            return NotFound(new { status = false, message = "Người dùng không tồn tại" });

        var result = await _userService.UpdateUserRoleAsync(user, dto.Role);
        if (!result.Succeeded)
            return BadRequest(new { status = false, message = "Cập nhật quyền thất bại", errors = result.Errors });

        return Ok(new { status = true, message = "Cập nhật quyền thành công" });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteUser([FromBody] DeleteUserRequest request)
    {
        var user = await _userService.GetByUserNameAsync(request.UserName);
        if (user == null)
        {
            return NotFound(new { status = false, message = "Người dùng không tồn tại." });
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(new { status = false, message = "Xóa thất bại", errors = result.Errors });
        }

        return Ok(new { status = true, message = "Đã xóa tài khoản thành công!" });
    }

    [Authorize(Roles = "Employee,Admin")]
    [HttpGet("get-all-roles")]
    public async Task<IActionResult> GetAllRoles()
    {
        var roles = await _userService.GetAllRolesAsync();
        return Ok(new { status = true, data = roles });
    }



    //[HttpPost("forgot-password")]
    //public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    //{
    //    var user = await _userService.GetByEmailAsync(request.Email);
    //    if (user == null) return NotFound();
    //    var token = await _userService.GeneratePasswordResetTokenAsync(user);
    //    return Ok(new { status = true, token });
    //}

    //[HttpPost("reset-password")]
    //public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    //{
    //    var user = await _userService.GetByEmailAsync(request.Email);
    //    if (user == null) return NotFound();
    //    var success = await _userService.ResetPasswordAsync(user, request.Token, request.NewPassword);
    //    return success ? Ok(new { status = true }) : BadRequest(new { status = false });
    //}

    //[HttpPut("update-info")]
    //public async Task<IActionResult> UpdateInfo([FromBody] UpdateUserInfoRequest request)
    //{
    //    var user = await _userService.GetByIdAsync(request.Id);
    //    if (user == null) return NotFound();
    //    user.FullName = request.FullName;
    //    user.Address = request.Address;
    //    user.PhoneNumber = request.PhoneNumber;
    //    var success = await _userService.UpdateUserInfoAsync(user);
    //    return success ? Ok(new { status = true }) : BadRequest(new { status = false });
    //}

}
