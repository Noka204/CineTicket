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
    public class PhongChieuController : ControllerBase
    {
        private readonly IPhongChieuService _phongService;
        private readonly IMapper _mapper;
        private readonly IGheService _gheService;

        public PhongChieuController(IPhongChieuService phongService, IMapper mapper, IGheService gheService )
        {
            _phongService = phongService;
            _mapper = mapper;
            _gheService = gheService;

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

        [Authorize(Roles = "Employee,Admin")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreatePhongChieuRequest request)
        {
            var result = await _phongService.CreateWithSeatsAsync(request);

            return CreatedAtAction(nameof(GetById), new
            {
                id = result.MaPhong
            }, new
            {
                status = true,
                message = "Tạo phòng chiếu thành công kèm danh sách ghế",
                data = result
            });
        }

        [Authorize(Roles = "Employee,Admin")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePhongChieuRequest request)
        {
            if (id != request.MaPhong)
                return BadRequest(new { status = false, message = "Mã phòng không khớp", data = (object?)null });

            var phong = _mapper.Map<PhongChieu>(request);
            var updated = await _phongService.UpdateAsync(phong);

            if (updated)
                return NoContent(); 
            else
                return NotFound(new { status = false, message = "Không tìm thấy phòng để cập nhật", data = (object?)null });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _phongService.DeleteAsync(id);
            if (deleted)
                return NoContent();
            else
                return NotFound(new { status = false, message = "Không tìm thấy phòng để xoá", data = (object?)null });
        }
    }
}
