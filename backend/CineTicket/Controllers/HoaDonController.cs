using System.Security.Claims;
using AutoMapper;
using CineTicket.Data;
using CineTicket.DTOs.HoaDon;
using CineTicket.DTOs.Mail;
using CineTicket.Models;
using CineTicket.Repositories.Implementations;
using CineTicket.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class HoaDonController : ControllerBase
{
    private readonly IHoaDonService _service;
    private readonly IMapper _mapper;
    private readonly CineTicketDbContext _context;
    private readonly MailService _mailService;
    private readonly IHoaDonService _hoaDonService;

    public HoaDonController(IHoaDonService service, IMapper mapper, CineTicketDbContext context, MailService mailService, IHoaDonService hoaDonService)
    {
        _service = service;
        _mapper = mapper;
        _context = context;
        _mailService = mailService;
        _hoaDonService = hoaDonService;
    }

    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll()
    {
        var list = await _service.GetAllAsync();
        return Ok(new { status = true, data = _mapper.Map<IEnumerable<HoaDonDTO>>(list) });
    }

    [HttpGet("get/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null)
            return NotFound(new { status = false, message = "Không tìm thấy hóa đơn" });

        return Ok(new { status = true, data = _mapper.Map<HoaDonDTO>(item) });
    }
    [AllowAnonymous]
    [HttpPost("send-invoice")]
    public async Task<IActionResult> SendInvoice([FromBody] SendInvoiceRequest body)
    {
        if (body == null || body.MaHd <= 0)
            return BadRequest(new { status = false, message = "maHD không hợp lệ" });

        try
        {
            await _mailService.SendInvoiceEmailAsync(body.MaHd);
            return Ok(new { status = true, message = "Đã gửi (nếu hóa đơn tồn tại)" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = false, message = "Gửi email thất bại" });
        }
    }

    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateHoaDonDTO request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { status = false, message = "Không xác định được người dùng" });

            var created = await _hoaDonService.CreateWithDetailsAsync(request, userId);

            return CreatedAtAction(nameof(GetById), new { id = created.MaHd }, new
            {
                status = true,
                data = new
                {
                    created.MaHd,
                    created.NgayLap,
                    created.TongTien,
                    amount = created.TongTien, // <— thêm
                    created.TrangThai,
                    created.HinhThucThanhToan,
                    created.ApplicationUserId,
                    created.ChiTietHoaDons
                }
            });
        }
        catch(Exception ex)
        {
            return BadRequest(new { status = false, message = ex.Message });
        }
    }

    [Authorize(Roles = "Employee,Admin")]
    [HttpPut("update/{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateHoaDonDTO request)
    {
        if (id != request.MaHd)
            return BadRequest(new { status = false, message = "Mã hóa đơn không khớp" });

        var model = _mapper.Map<HoaDon>(request);
        var success = await _service.UpdateAsync(request);

        return success ? NoContent() : NotFound(new { status = false, message = "Không tìm thấy hóa đơn để cập nhật" });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success)
            return NotFound(new { status = false, message = "Không tìm thấy hóa đơn để xoá" });

        return Ok(new { status = true, message = "Xoá hóa đơn thành công" });
    }
    [Authorize]
    [HttpGet("my/paid-movies")]
    public async Task<IActionResult> MyPaidMovies([FromQuery] int skip = 0, [FromQuery] int take = 8)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { status = false, message = "Không xác định được người dùng" });

        var (total, items) = await _hoaDonService.GetMyPaidMoviesAsync(userId, skip, take);
        return Ok(new { status = true, data = new { total, items } });
    }

}