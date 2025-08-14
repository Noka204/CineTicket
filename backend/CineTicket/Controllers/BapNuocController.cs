using AutoMapper;
using CineTicket.DTOs;
using CineTicket.Models;
using CineTicket.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineTicket.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BapNuocController : ControllerBase
{
    private readonly IBapNuocService _service;
    private readonly IMapper _mapper;

    public BapNuocController(IBapNuocService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll()
    {
        var list = await _service.GetAllAsync();
        var mapped = _mapper.Map<IEnumerable<BapNuocDTO>>(list);
        return Ok(new { status = true, message = "Lấy danh sách thành công", data = mapped });
    }

    [HttpGet("get/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var bn = await _service.GetByIdAsync(id);
        if (bn == null)
            return NotFound(new { status = false, message = "Không tìm thấy bắp nước", data = (object?)null });

        var mapped = _mapper.Map<BapNuocDTO>(bn);
        return Ok(new { status = true, message = "Lấy thành công", data = mapped });
    }

    //[Authorize(Roles = "Employee,Admin")]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] BapNuocCreateDTO request)
    {
        var model = _mapper.Map<BapNuoc>(request);
        var created = await _service.CreateAsync(model);
        var mapped = _mapper.Map<BapNuocDTO>(created);

        return CreatedAtAction(nameof(GetById), new { id = mapped.MaBn }, new { status = true, message = "Tạo bắp nước thành công", data = mapped });
    }

    [Authorize(Roles = "Employee,Admin")]
    [HttpPut("update/{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] BapNuocUpdateDTO request)
    {
        if (id != request.MaBn)
            return BadRequest(new { status = false, message = "Mã bắp nước không khớp", data = (object?)null });
        var model = _mapper.Map<BapNuoc>(request);
        var updated = await _service.UpdateAsync(model);
        if (updated)
            return NoContent();
        else
            return NotFound(new { status = false, message = "Không tìm thấy bắp nước để cập nhật", data = (object?)null });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (deleted)
            return NoContent();
        else
            return NotFound(new { status = false, message = "Không tìm thấy bắp nước để xóa", data = (object?)null });
    }
}
