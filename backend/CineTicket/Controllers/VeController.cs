using System.Security.Claims;
using AutoMapper;
using CineTicket.Data;
using CineTicket.DTOs;
using CineTicket.DTOs.Ve;
using CineTicket.Models;
using CineTicket.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static CineTicket.Services.Interfaces.IVeService;

namespace CineTicket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VeController : ControllerBase
    {
        private readonly IVeService _service;
        private readonly IMapper _mapper;
        private readonly CineTicketDbContext _context;

        public VeController(IVeService service, IMapper mapper, CineTicketDbContext context)
        {
            _service = service;
            _mapper = mapper;
            _context = context;

        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            var mapped = _mapper.Map<IEnumerable<VeDTO>>(result);
            return Ok(new { status = true, message = "Lấy danh sách vé thành công", data = mapped });
        }
        [Authorize]
        [HttpGet("tinh-gia-ve")]
        public async Task<IActionResult> TinhGiaVe(int maGhe, int maSuat)
        {
            var result = await _service.TinhGiaVeAsync(maGhe, maSuat);

            if (result == null)
            {
                return BadRequest(new
                {
                    status = false,
                    message = "Không tìm thấy ghế hoặc suất chiếu."
                });
            }
            return Ok(new
            {
                status = true,
                message = "Tính giá vé thành công",
                data = result
            });
        }
        [Authorize]
        [HttpPost("hold")]
        public async Task<IActionResult> GiuGhe([FromBody] GiuGheRequest request)
        {
            string? userId = null;
            if (User?.Identity?.IsAuthenticated == true)
                userId = User.FindFirst("sub")?.Value
                      ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var (success, message, data, statusCode) = await _service.GiuGheAsync(request, userId);
            if (!success) return StatusCode(statusCode, new { status = false, message });
            return StatusCode(statusCode, new { status = true, message, data });
        }
        [Authorize]
        [HttpGet("held-by-me")]
        [Authorize]
        public async Task<IActionResult> GetHeldByMe([FromQuery] int maSuat)
        {
            if (maSuat <= 0)
                return BadRequest(new { status = false, message = "maSuat invalid" });

            var userId =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                User.FindFirst("UserId")?.Value ??
                User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { status = false, message = "No user id" });

            var list = await _service.GetHeldByUserAsync(maSuat, userId);
            return Ok(new { status = true, data = list }); // list: các ghế tôi đang giữ
        }
        [Authorize]
        [HttpPost("release")]
        public async Task<IActionResult> BoGiuGhe([FromBody] GiuGheRequest request)
        {
            string? userId = null;
            if (User?.Identity?.IsAuthenticated == true)
                userId = User.FindFirst("sub")?.Value
                      ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var (success, message) = await _service.BoGiuGheAsync(request, userId);
            if (!success) return NotFound(new { status = false, message });
            return Ok(new { status = true, message });
        }
        
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateVeRequest request)
        {
            var model = _mapper.Map<Ve>(request);
            var created = await _service.CreateAsync(model);
            var mapped = _mapper.Map<VeDTO>(created);

            // KHỚP với action GetByIdAsync ở trên
            return CreatedAtAction(nameof(_service.GetByIdAsync), new { id = mapped.MaVe },
                new { status = true, message = "Tạo vé thành công", data = mapped });
        }
        [Authorize(Roles = "Employee,Admin")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateVeRequest request)
        {
            if (id != request.MaVe)
                return BadRequest(new { status = false, message = "Mã vé không khớp", data = (object?)null });

            var model = _mapper.Map<Ve>(request);
            var updated = await _service.UpdateAsync(model);

            if (updated)
                return NoContent();
            else
                return NotFound(new { status = false, message = "Không tìm thấy vé để cập nhật", data = (object?)null });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (deleted)
                return NoContent();
            else
                return NotFound(new { status = false, message = "Không tìm thấy vé để xoá", data = (object?)null });
        }
    }
}
