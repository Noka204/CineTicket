using AutoMapper;
using CineTicket.DTOs.Accout;
using CineTicket.DTOs.Auth;
using CineTicket.Models;
using CineTicket.Services;
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
    private static Dictionary<string, (string otp, DateTime expires)> OtpStore = new();
    private readonly MailService _mailService;


    public AccountController(IUserService userService, IMapper mapper, IConfiguration config, UserManager<ApplicationUser> userManager, JwtTokenGenerator jwtTokenGenerator,MailService mailService)
    {
        _userService = userService;
        _mapper = mapper;
        _config = config;
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _mailService = mailService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        var user = _mapper.Map<ApplicationUser>(request);
        user.Role = SD.Role_Customer;
        var result = await _userService.RegisterAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                status = false,
                message = string.Join("; ", result.Errors.Select(e => e.Description))
            });
        }
        await _userService.AssignRoleAsync(user, SD.Role_Customer);
        var createdUser = await _userService.GetByUserNameAsync(request.UserName);
        var token = _jwtTokenGenerator.GenerateToken(createdUser);
        return Ok(new
        {
            status = true,
            message = "Đăng ký thành công",
            token = token
        });
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
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var email = request.Email;
        var user = await _userService.GetByEmailAsync(email);
        if (user == null)
            return NotFound(new { status = false, message = "Email không tồn tại trong hệ thống." });

        var otp = new Random().Next(100000, 999999).ToString();
        await _mailService.SendOtpEmailAsync(email, otp);
        OtpStore[email] = (otp, DateTime.UtcNow.AddMinutes(5));

        return Ok(new { status = true, message = "Mã OTP đã được gửi đến email của bạn." });
    }
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (!OtpStore.TryGetValue(request.Email, out var otpInfo))
            return BadRequest(new { status = false, message = "OTP không tồn tại hoặc email sai." });

        if (DateTime.UtcNow > otpInfo.expires)
            return BadRequest(new { status = false, message = "Mã OTP đã hết hạn." });

        if (otpInfo.otp != request.Otp)
            return BadRequest(new { status = false, message = "Mã OTP không đúng." });

        var user = await _userService.GetByEmailAsync(request.Email);
        if (user == null)
            return NotFound(new { status = false, message = "Không tìm thấy người dùng." });

        var result = await _userService.ResetPasswordAsync(user, request.NewPassword);
        if (!result.Succeeded)
            return BadRequest(new { status = false, message = "Không thể đặt lại mật khẩu." });

        OtpStore.Remove(request.Email);
        return Ok(new { status = true, message = "Đặt lại mật khẩu thành công." });
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
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe([FromQuery] string? userName = null, [FromQuery] string? userId = null)
    {
        // 1) Lấy user từ token (ưu tiên Id)
        var tokenUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue("sub")
                          ?? User.FindFirstValue("uid");

        var tokenUserName = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;

        ApplicationUser? user = null;

        if (!string.IsNullOrWhiteSpace(tokenUserId))
            user = await _userManager.FindByIdAsync(tokenUserId);

        if (user == null && !string.IsNullOrWhiteSpace(tokenUserName))
            user = await _userManager.FindByNameAsync(tokenUserName!);

        if (user == null)
            return Unauthorized(new { status = false, message = "Không lấy được người dùng từ token." });

        if (!string.IsNullOrWhiteSpace(userName) &&
            !string.Equals(user.UserName, userName, StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(403, new { status = false, message = "Token không thuộc về user được yêu cầu." });
        }

        if (!string.IsNullOrWhiteSpace(userId) &&
            !string.Equals(user.Id, userId, StringComparison.Ordinal))
        {
            return StatusCode(403, new { status = false, message = "Token không thuộc về user được yêu cầu." });
        }

        var data = new
        {
            fullName = user.FullName,
            userName = user.UserName,
            address = user.Address,    
            email = user.Email,
            sdt = user.PhoneNumber 
        };

        return Ok(new { status = true, data });
    }
    [Authorize]
    [HttpPut("update-me")]
    public async Task<IActionResult> UpdateMe(
    [FromBody] UpdateProfileRequest request,
    [FromQuery] string? userName = null,
    [FromQuery] string? userId = null)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                status = false,
                message = "Dữ liệu không hợp lệ.",
                errors = ModelState
                    .Where(kv => kv.Value?.Errors?.Count > 0)
                    .ToDictionary(
                        kv => kv.Key,
                        kv => kv.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    )
            });
        }

        var tokenUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue("sub")
                          ?? User.FindFirstValue("uid");
        var tokenUserName = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;

        ApplicationUser? user = null;
        if (!string.IsNullOrWhiteSpace(tokenUserId))
            user = await _userManager.FindByIdAsync(tokenUserId);
        if (user == null && !string.IsNullOrWhiteSpace(tokenUserName))
            user = await _userManager.FindByNameAsync(tokenUserName!);

        if (user == null)
            return Unauthorized(new { status = false, message = "Không lấy được người dùng từ token." });

        if (!string.IsNullOrWhiteSpace(userName) &&
            !string.Equals(user.UserName, userName, StringComparison.OrdinalIgnoreCase))
            return StatusCode(403, new { status = false, message = "Token không thuộc về user được yêu cầu." });

        if (!string.IsNullOrWhiteSpace(userId) &&
            !string.Equals(user.Id, userId, StringComparison.Ordinal))
            return StatusCode(403, new { status = false, message = "Token không thuộc về user được yêu cầu." });

        var changed = false;

        if (!string.IsNullOrWhiteSpace(request.FullName) && request.FullName!.Trim() != user.FullName)
        {
            user.FullName = request.FullName.Trim();
            changed = true;
        }

        if (request.Address != null && request.Address != user.Address)
        {
            user.Address = request.Address?.Trim();
            changed = true;
        }

        if (request.Sdt != null && request.Sdt != user.PhoneNumber)
        {
            user.PhoneNumber = request.Sdt?.Trim();
            changed = true;
        }

        if (!string.IsNullOrWhiteSpace(request.Email) && !string.Equals(request.Email, user.Email, StringComparison.OrdinalIgnoreCase))
        {
            var exists = await _userService.GetByEmailAsync(request.Email);
            if (exists != null && exists.Id != user.Id)
                return Conflict(new { status = false, message = "Email đã được sử dụng bởi tài khoản khác." });

            var setEmail = await _userManager.SetEmailAsync(user, request.Email!.Trim());
            if (!setEmail.Succeeded)
                return BadRequest(new { status = false, message = "Không thể cập nhật email.", errors = setEmail.Errors });

            changed = true;
        }

        if (!changed)
        {
            return Ok(new
            {
                status = true,
                message = "Không có thay đổi.",
                data = new
                {
                    fullName = user.FullName,
                    userName = user.UserName,
                    address = user.Address,
                    email = user.Email,
                    sdt = user.PhoneNumber
                }
            });
        }

        var updated = await _userService.UpdateUserInfoAsync(user);
        if (!updated)
            return BadRequest(new { status = false, message = "Cập nhật thất bại." });

        return Ok(new
        {
            status = true,
            message = "Cập nhật thành công.",
            data = new
            {
                fullName = user.FullName,
                userName = user.UserName,
                address = user.Address,
                email = user.Email,
                sdt = user.PhoneNumber
            }
        });
    }
}
