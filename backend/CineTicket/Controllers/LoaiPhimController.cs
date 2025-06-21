using AutoMapper;
using CineTicket.DTOs.LoaiPhim;
using CineTicket.Models;
using CineTicket.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CineTicket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoaiPhimController : ControllerBase
    {
        private readonly ILoaiPhimService _service;
        private readonly IMapper _mapper;

        public LoaiPhimController(ILoaiPhimService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(new { status = true, data = _mapper.Map<IEnumerable<LoaiPhimDTO>>(list) });
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null)
                return NotFound(new { status = false, message = "Không tìm thấy" });
            return Ok(new { status = true, data = _mapper.Map<LoaiPhimDTO>(item) });
        }
        [Authorize(Roles = "Employee,Admin")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateLoaiPhimRequest request)
        {
            var model = _mapper.Map<LoaiPhim>(request);
            var created = await _service.CreateAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = created.MaLoaiPhim }, new { status = true, data = _mapper.Map<LoaiPhimDTO>(created) });
        }
        [Authorize(Roles = "Employee,Admin")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLoaiPhimRequest request)
        {
            if (id != request.MaLoaiPhim)
                return BadRequest(new { status = false, message = "Id không khớp" });

            var model = _mapper.Map<LoaiPhim>(request);
            var success = await _service.UpdateAsync(model);

            return success ? NoContent() : NotFound(new { status = false, message = "Không tìm thấy để cập nhật" });
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            return success ? NoContent() : NotFound(new { status = false, message = "Không tìm thấy để xoá" });
        }
    }
}
