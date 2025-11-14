using CineTicket.DTOs.KhuyenMai;
using CineTicket.Models;
using CineTicket.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CineTicket.Controllers
{
    [ApiController]
    [Route("api/khuyenmai")]
    public class KhuyenMaiPublicController : ControllerBase
    {
        private readonly IKhuyenMaiService _svc;
        private readonly UserManager<ApplicationUser> _userManager;

        public KhuyenMaiPublicController(
            IKhuyenMaiService svc,
            UserManager<ApplicationUser> userManager)
        {
            _svc = svc;
            _userManager = userManager;
        }
        // GET /api/khuyenmai/validate?code=ABC123
        [HttpGet("validate")]
        [AllowAnonymous]
        public async Task<IActionResult> Validate([FromQuery] string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest(new { status = false, message = "Ma khong hop le." });

            string? userId = null;
            if (User?.Identity?.IsAuthenticated == true)
                userId = _userManager.GetUserId(User);

            // amount = null khi chỉ kiểm tra hợp lệ
            var rs = await _svc.ValidateAsync(code, null, userId);

            if (!rs.Success)
                return Ok(new { status = false, message = rs.Message ?? "Ma khong hop le hoac da dung." });

            var d = rs.Data!; // đã Success => d chắc chắn có
            return Ok(new
            {
                status = true,
                data = new
                {
                    khuyenMaiId = d.KhuyenMaiId,
                    ten = d.Ten,                      // nhớ đảm bảo DTO có Ten
                    loaiGiam = d.LoaiGiam.ToString(),     // Percent | Amount
                    mucGiam = d.MucGiam,
                    code = d.Code,                     // hoặc CodeUsed nếu bạn đặt tên vậy
                    batDau = d.BatDau,
                    ketThuc = d.KetThuc
                }
            });
        }


        [AllowAnonymous]
        [HttpPost("apply")]
        public async Task<IActionResult> Apply([FromBody] ApplyCouponRequestDto dto)
        {
            var userId = User?.Identity?.IsAuthenticated == true
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;

            var rs = await _svc.ValidateAsync(dto.Code, dto.Amount, userId);
            if (!rs.Success)
                return BadRequest(new { status = false, message = rs.Message });

            return Ok(new { status = true, data = rs.Data });
        }
    }
}
