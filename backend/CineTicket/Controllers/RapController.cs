using System.Linq;
using System.Threading.Tasks;
using CineTicket.DTOs.Rap;
using CineTicket.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineTicket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RapController : ControllerBase
    {
        private readonly IRapService _service;
        public RapController(IRapService service) => _service = service;

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
            => Ok(new { status = true, message = "OK", data = await _service.GetAllAsync() });

        [HttpGet("get/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var x = await _service.GetByIdAsync(id);
            if (x is null) return NotFound(new { status = false, message = "Không tìm thấy", data = (object?)null });
            return Ok(new { status = true, message = "OK", data = x });
        }

        [Authorize(Roles = "Employee,Admin")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] RapCreateDTO dto)
        {
            var x = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = x.MaRap }, new { status = true, message = "Tạo thành công", data = x });
        }

        [Authorize(Roles = "Employee,Admin")]
        [HttpPut("update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] RapUpdateDTO dto)
        {
            var x = await _service.UpdateAsync(id, dto);
            if (x is null) return NotFound(new { status = false, message = "Không tìm thấy", data = (object?)null });
            return Ok(new { status = true, message = "Cập nhật thành công", data = x });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) return NotFound(new { status = false, message = "Không tìm thấy", data = (object?)null });
            return NoContent();
        }

        // ===== APIs cho FE
        [HttpGet("get-cities")]
        public async Task<IActionResult> GetCities()
            => Ok(new { status = true, message = "OK", data = await _service.GetCitiesAsync() });

        [HttpGet("by-city")]
        public async Task<IActionResult> GetByCity([FromQuery] string thanhPho)
            => Ok(new { status = true, message = "OK", data = await _service.GetByCityAsync(thanhPho) });
    }
}
