using CineTicket.DTOs.KhuyenMai;
using CineTicket.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineTicket.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/khuyenmai")]
    [Authorize(Roles = "Admin")]
    public class KhuyenMaiController : ControllerBase
    {
        private readonly IKhuyenMaiService _svc;
        public KhuyenMaiController(IKhuyenMaiService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _svc.GetAllAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var km = await _svc.GetAsync(id);
            return km is null ? NotFound() : Ok(km);
        }
        [Authorize(Roles = "Employee,Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] KhuyenMaiCreateDto dto)
        {
            var km = await _svc.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = km.Id }, km);
        }
        [Authorize(Roles = "Employee,Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] KhuyenMaiUpdateDto dto)
        {
            if (id != dto.Id) return BadRequest("Id không khớp.");
            var km = await _svc.GetAsync(id);
            if (km is null) return NotFound();

            var updated = await _svc.UpdateAsync(dto);
            return Ok(updated);
        }
        [Authorize(Roles = "Employee,Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _svc.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }
    }
}
