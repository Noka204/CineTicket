using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CineTicket.Models;
using CineTicket.Services.Interfaces;
using AutoMapper;
using CineTicket.DTOs;

namespace CineTicket.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhimController : ControllerBase
    {
        private readonly IPhimService _phimService;
        private readonly IMapper _mapper;

        public PhimController(IPhimService phimService, IMapper mapper)
        {
            _phimService = phimService;
            _mapper = mapper;
        }

        // GET: api/phim/get-all
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            var danhSach = await _phimService.GetAllAsync();
            var mapped = _mapper.Map<IEnumerable<PhimDTO>>(danhSach);
            return Ok(new { status = true, message = "Lấy danh sách phim thành công", data = mapped });
        }

        // GET: api/phim/get/{id}
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var phim = await _phimService.GetByIdAsync(id);
            if (phim == null)
                return NotFound(new { status = false, message = "Không tìm thấy phim", data = (object?)null });

            var mapped = _mapper.Map<PhimDTO>(phim);
            return Ok(new { status = true, message = "Lấy phim thành công", data = mapped });
        }

        // POST: api/phim/create
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreatePhimRequest request)
        {
            var model = _mapper.Map<Phim>(request);
            var createdPhim = await _phimService.CreateAsync(model);
            var mapped = _mapper.Map<PhimDTO>(createdPhim);

            return CreatedAtAction(nameof(GetById), new { id = mapped.MaPhim }, new { status = true, message = "Tạo phim thành công", data = mapped });
        }

        // PUT: api/phim/update/{id}
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePhimRequest request)
        {
            if (id != request.MaPhim)
                return BadRequest(new { status = false, message = "Mã phim không khớp", data = (object?)null });

            var model = _mapper.Map<Phim>(request);
            var success = await _phimService.UpdateAsync(model);

            if (success)
                return NoContent();
            else
                return NotFound(new { status = false, message = "Không tìm thấy phim để cập nhật", data = (object?)null });
        }

        // DELETE: api/phim/delete/{id}
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _phimService.DeleteAsync(id);
            if (success)
                return NoContent();
            else
                return NotFound(new { status = false, message = "Không tìm thấy phim để xoá", data = (object?)null });
        }
    }
}
