using AutoMapper;
using CineTicket.DTOs;
using CineTicket.Models;
using CineTicket.Services.Implementations;
using CineTicket.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineTicket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GheController : ControllerBase
    {
        private readonly IGheService _service;
        private readonly IMapper _mapper;

        public GheController(IGheService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            var mapped = _mapper.Map<IEnumerable<GheDTO>>(result);
            return Ok(new { status = true, message = "Lấy danh sách ghế thành công", data = mapped });
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var ghe = await _service.GetByIdAsync(id);
            if (ghe == null)
                return NotFound(new { status = false, message = "Không tìm thấy ghế", data = (object?)null });

            var mapped = _mapper.Map<GheDTO>(ghe);
            return Ok(new { status = true, message = "Lấy ghế thành công", data = mapped });
        }
        [HttpGet("get-by-phong")]
        public async Task<IActionResult> GetByPhong([FromQuery] int maPhong)
        {
            var list = await _service.GetByPhongAsync(maPhong); // <-- PHẢI TRẢ VỀ IEnumerable<Ghe>
            var data = list.Select(g => new GheDTO
            {
                MaGhe = g.MaGhe,
                SoGhe = g.SoGhe,
                LoaiGhe = g.LoaiGhe,
                MaPhong = g.MaPhong,
                TenPhong = g.MaPhongNavigation?.TenPhong,
            });
            return Ok(new { status = true, message = "OK", data });
        }
        // GheController.cs
        [HttpGet("get-trang-thai")]
        public async Task<IActionResult> GetTrangThai([FromQuery] int maPhong, [FromQuery] int maSuat)
        {
            var data = await _service.GetTrangThaiGheAsync(maPhong, maSuat);
            return Ok(new { status = true, data });
        }

        [Authorize(Roles = "Employee,Admin")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateGheRequest request)
        {
            var model = _mapper.Map<Ghe>(request);
            var created = await _service.CreateAsync(model);
            var mapped = _mapper.Map<GheDTO>(created);

            return CreatedAtAction(nameof(GetById), new { id = mapped.MaGhe }, new { status = true, message = "Tạo ghế thành công", data = mapped });
        }
        [Authorize(Roles = "Employee,Admin")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateGheRequest request)
        {
            if (id != request.MaGhe)
                return BadRequest(new { status = false, message = "Mã ghế không khớp", data = (object?)null });

            var model = _mapper.Map<Ghe>(request);
            var updated = await _service.UpdateAsync(model);

            if (updated)
                return NoContent();
            else
                return NotFound(new { status = false, message = "Không tìm thấy ghế để cập nhật", data = (object?)null });
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (deleted)
                return NoContent();
            else
                return NotFound(new { status = false, message = "Không tìm thấy ghế để xoá", data = (object?)null });
        }
    }
}
