using AutoMapper;
using System.Linq;

using CineTicket.DTOs;
using CineTicket.Models;
using CineTicket.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineTicket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuatChieuController : ControllerBase
    {
        private readonly ISuatChieuService _service;
        private readonly IMapper _mapper;

        public SuatChieuController(ISuatChieuService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            var mapped = _mapper.Map<IEnumerable<SuatChieuDTO>>(result);

            return Ok(new { status = true, message = "Lấy danh sách suất chiếu thành công", data = mapped });
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var suat = await _service.GetByIdAsync(id);
            if (suat == null)
                return NotFound(new { status = false, message = "Không tìm thấy suất chiếu", data = (object?)null });

            var mapped = _mapper.Map<SuatChieuDTO>(suat);
            return Ok(new { status = true, message = "Lấy suất chiếu thành công", data = mapped });
        }

        [HttpGet("get-by-phim/{maPhim}")]
        public async Task<IActionResult> GetByPhimId(int maPhim)
        {
            var suatChieus = await _service.GetByPhimIdAsync(maPhim);
            if (suatChieus == null || !suatChieus.Any())
                return NotFound(new { status = false, message = "Không tìm thấy suất chiếu cho phim này", data = (object?)null });

            var mapped = _mapper.Map<IEnumerable<SuatChieuDTO>>(suatChieus);
            return Ok(new { status = true, message = "Lấy danh sách suất chiếu theo phim thành công", data = mapped });
        }


        [Authorize(Roles = "Employee,Admin")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateSuatChieuRequest request)
        {
            var model = _mapper.Map<SuatChieu>(request);
            var created = await _service.CreateAsync(model);
            var mapped = _mapper.Map<SuatChieuDTO>(created);

            return CreatedAtAction(nameof(GetById), new { id = mapped.MaSuat }, new { status = true, message = "Tạo suất chiếu thành công", data = mapped });
        }

        [Authorize(Roles = "Employee,Admin")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSuatChieuRequest request)
        {
            if (id != request.MaSuat)
                return BadRequest(new { status = false, message = "Mã suất chiếu không khớp", data = (object?)null });

            var model = _mapper.Map<SuatChieu>(request);
            var updated = await _service.UpdateAsync(model);

            if (updated)
                return NoContent(); // REST chuẩn
            else
                return NotFound(new { status = false, message = "Không tìm thấy suất chiếu để cập nhật", data = (object?)null });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (deleted)
                return NoContent(); // REST chuẩn
            else
                return NotFound(new { status = false, message = "Không tìm thấy suất chiếu để xoá", data = (object?)null });
        }
    }
}
