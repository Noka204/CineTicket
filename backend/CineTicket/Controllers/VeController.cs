using AutoMapper;
using CineTicket.DTOs;
using CineTicket.Models;
using CineTicket.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CineTicket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VeController : ControllerBase
    {
        private readonly IVeService _service;
        private readonly IMapper _mapper;

        public VeController(IVeService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            var mapped = _mapper.Map<IEnumerable<VeDTO>>(result);
            return Ok(new { status = true, message = "Lấy danh sách vé thành công", data = mapped });
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var ve = await _service.GetByIdAsync(id);
            if (ve == null)
                return NotFound(new { status = false, message = "Không tìm thấy vé", data = (object?)null });

            var mapped = _mapper.Map<VeDTO>(ve);
            return Ok(new { status = true, message = "Lấy vé thành công", data = mapped });
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateVeRequest request)
        {
            var model = _mapper.Map<Ve>(request);
            var created = await _service.CreateAsync(model);
            var mapped = _mapper.Map<VeDTO>(created);

            return CreatedAtAction(nameof(GetById), new { id = mapped.MaVe }, new { status = true, message = "Tạo vé thành công", data = mapped });
        }

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
