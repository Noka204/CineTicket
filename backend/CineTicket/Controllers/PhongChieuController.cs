using AutoMapper;
using CineTicket.DTOs;
using CineTicket.Models;
using CineTicket.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CineTicket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhongChieuController : ControllerBase
    {
        private readonly IPhongChieuService _phongService;
        private readonly IMapper _mapper;

        public PhongChieuController(IPhongChieuService phongService, IMapper mapper)
        {
            _phongService = phongService;
            _mapper = mapper;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _phongService.GetAllAsync();
            var mapped = _mapper.Map<IEnumerable<PhongChieuDTO>>(result);
            return Ok(new { status = true, message = "Lấy danh sách phòng chiếu thành công", data = mapped });
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var phong = await _phongService.GetByIdAsync(id);
            if (phong == null)
                return NotFound(new { status = false, message = "Không tìm thấy phòng chiếu", data = (object?)null });

            var mapped = _mapper.Map<PhongChieuDTO>(phong);
            return Ok(new { status = true, message = "Lấy phòng chiếu thành công", data = mapped });
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreatePhongChieuRequest request)
        {
            var phong = _mapper.Map<PhongChieu>(request);
            var created = await _phongService.CreateAsync(phong);
            var mapped = _mapper.Map<PhongChieuDTO>(created);

            // Theo REST chuẩn, khi tạo mới nên trả 201 Created
            return CreatedAtAction(nameof(GetById), new { id = mapped.MaPhong }, new { status = true, message = "Tạo phòng chiếu thành công", data = mapped });
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePhongChieuRequest request)
        {
            if (id != request.MaPhong)
                return BadRequest(new { status = false, message = "Mã phòng không khớp", data = (object?)null });

            var phong = _mapper.Map<PhongChieu>(request);
            var updated = await _phongService.UpdateAsync(phong);

            if (updated)
                return NoContent(); // Chuẩn REST: 204 NoContent khi update OK
            else
                return NotFound(new { status = false, message = "Không tìm thấy phòng để cập nhật", data = (object?)null });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _phongService.DeleteAsync(id);
            if (deleted)
                return NoContent(); // Chuẩn REST
            else
                return NotFound(new { status = false, message = "Không tìm thấy phòng để xoá", data = (object?)null });
        }
    }
}
