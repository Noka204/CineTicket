using AutoMapper;
using CineTicket.DTOs.HoaDon;
using CineTicket.Models;
using CineTicket.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class HoaDonController : ControllerBase
{
    private readonly IHoaDonService _service;
    private readonly IMapper _mapper;

    public HoaDonController(IHoaDonService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
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

    [Authorize(Roles = "Employee,Admin")]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateHoaDonDTO request)
    {
        var created = await _service.CreateWithDetailsAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.MaHd }, new { status = true, data = _mapper.Map<HoaDonDTO>(created) });
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
}
