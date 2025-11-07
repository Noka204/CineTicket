using CineTicket.DTOs.KhuyenMai;
using CineTicket.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineTicket.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/khuyenmaicode")]
    [Authorize(Roles = "Admin")]
    public class KhuyenMaiCodeController : ControllerBase
    {
        private readonly IKhuyenMaiCodeService _svc;
        public KhuyenMaiCodeController(IKhuyenMaiCodeService svc) => _svc = svc;

        [HttpGet("by-km/{khuyenMaiId:int}")]
        public async Task<IActionResult> GetByPromotion(int khuyenMaiId) =>
            Ok(await _svc.GetByPromotionAsync(khuyenMaiId));

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var code = await _svc.GetByIdAsync(id);
            return code is null ? NotFound() : Ok(code);
        }
        [Authorize(Roles = "Employee,Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] KhuyenMaiCodeCreateDto dto)
        {
            if ((dto.Count ?? 0) > 0)
            {
                var list = await _svc.BulkGenerateAsync(dto.KhuyenMaiId, dto.Count!.Value, dto.Prefix, dto.AssignedToUserId);
                return Ok(list);
            }
            var c = await _svc.CreateAsync(dto);
            return Ok(c);
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
