using AutoMapper;
using CineTicket.Models;
using CineTicket.Services;
using CineTicket.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineTicket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BapNuocController : ControllerBase
    {
        private readonly IBapNuocService _service;
        private readonly IMapper _mapper;
        private readonly FileService _fileService;

        public BapNuocController(IBapNuocService service, IMapper mapper, FileService fileService)
        {
            _service = service;
            _mapper = mapper;
            _fileService = fileService;
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll(
                [FromQuery] string? lang = null,
                CancellationToken ct = default)
        {
            var target = NormalizeLang(lang);
            var list = await _service.GetAllAsync(target, ct);

            return Ok(new
            {
                status = true,
                message = "Lấy danh sách thành công",
                data = list
            });
        }

        private static string NormalizeLang(string? lang)
        {
            lang = (lang ?? "vi").Trim().ToLowerInvariant();
            return lang switch
            {
                "vi-vn" => "vi",
                "en-us" => "en",
                "fr-fr" => "fr",
                "" => "vi",
                _ => lang
            };
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var bn = await _service.GetByIdAsync(id);
            if (bn == null)
                return NotFound(new { status = false, message = "Không tìm thấy bắp nước", data = (object?)null });

            return Ok(new { status = true, message = "Lấy thành công", data = bn });
        }

        //[Authorize(Roles = "Employee,Admin")]
        [HttpPost("create")]
        public async Task<IActionResult> Create(
            [FromForm] string tenBn,
            [FromForm] decimal gia,
            [FromForm] string? moTa,
            [FromForm] IFormFile? image)
        {
            string? imgUrl = null;
            if (image != null)
            {
                imgUrl = await _fileService.SaveFileAsync(image, "BapNuoc");
            }

            var model = new BapNuoc
            {
                TenBn = tenBn,
                Gia = gia,
                MoTa = moTa,
                HinhAnhUrl = imgUrl
            };

            var created = await _service.CreateAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = created.MaBn },
                new { status = true, message = "Tạo bắp nước thành công", data = created });
        }

        [Authorize(Roles = "Employee,Admin")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(
            int id,
            [FromForm] int maBn,
            [FromForm] string tenBn,
            [FromForm] decimal gia,
            [FromForm] string? moTa,
            [FromForm] IFormFile? image)
        {
            if (id != maBn)
                return BadRequest(new { status = false, message = "Mã bắp nước không khớp", data = (object?)null });

            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { status = false, message = "Không tìm thấy bắp nước", data = (object?)null });

            string? imgUrl = existing.HinhAnhUrl;
            if (image != null)
            {
                imgUrl = await _fileService.SaveFileAsync(image, "BapNuoc");
            }

            existing.TenBn = tenBn;
            existing.Gia = gia;
            existing.MoTa = moTa;
            existing.HinhAnhUrl = imgUrl;

            var ok = await _service.UpdateAsync(existing);
            if (ok) return NoContent();
            return NotFound(new { status = false, message = "Không cập nhật được bắp nước", data = (object?)null });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (deleted) return NoContent();
            return NotFound(new { status = false, message = "Không tìm thấy bắp nước để xóa", data = (object?)null });
        }
    }
}
