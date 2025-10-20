using AutoMapper;
using CineTicket.DTOs;
using CineTicket.Models;
using CineTicket.Services;
using CineTicket.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhimController : ControllerBase
    {
        private readonly IPhimService _phimService;
        private readonly IMapper _mapper;
        private readonly FileService _fileService;

        public PhimController(IPhimService phimService, IMapper mapper, FileService fileService)
        {
            _phimService = phimService;
            _mapper = mapper;
            _fileService = fileService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            var danhSach = await _phimService.Query()
                .Include(p => p.ChiTietLoaiPhims)
                    .ThenInclude(ct => ct.LoaiPhim)
                .ToListAsync();

            var mapped = _mapper.Map<IEnumerable<PhimDTO>>(danhSach);
            return Ok(new { status = true, message = "Lấy danh sách phim thành công", data = mapped });
        }

        [HttpGet("get-phim-hot")]
        public async Task<IActionResult> GetPhimHot()
        {
            var hotPhim = await _phimService.Query()
                .Include(p => p.ChiTietLoaiPhims)
                    .ThenInclude(ct => ct.LoaiPhim)
                .Where(p => p.IsHot == "1")
                .ToListAsync();

            var mapped = _mapper.Map<IEnumerable<PhimDTO>>(hotPhim);
            return Ok(new { status = true, message = "Danh sách phim hot", data = mapped });
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var phim = await _phimService.Query()
                .Include(p => p.ChiTietLoaiPhims)
                    .ThenInclude(ct => ct.LoaiPhim)
                .FirstOrDefaultAsync(p => p.MaPhim == id);

            if (phim == null)
                return NotFound(new { status = false, message = "Không tìm thấy phim", data = (object?)null });

            var mapped = _mapper.Map<PhimDTO>(phim);
            return Ok(new { status = true, message = "Lấy phim thành công", data = mapped });
        }

        [Authorize(Roles = "Employee,Admin")]
        [HttpPost("create")]
        public async Task<IActionResult> Create(
            [FromForm] string tenPhim,
            [FromForm] int? thoiLuong,
            [FromForm] string? daoDien,
            [FromForm] string? ngonNgu,
            [FromForm] string? dienVien,
            [FromForm] string? khoiChieu,
            [FromForm] string? moTa,
            [FromForm] List<int> maLoaiPhims,
            [FromForm] IFormFile? poster,
            [FromForm] IFormFile? banner,
            [FromForm] string? trailer)
        {
            string? posterUrl = poster != null ? await _fileService.SaveFileAsync(poster, "Poster") : null;
            string? bannerUrl = banner != null ? await _fileService.SaveFileAsync(banner, "Banner") : null;

            var phim = new Phim
            {
                TenPhim = tenPhim,
                ThoiLuong = thoiLuong,
                DaoDien = daoDien,
                NgonNgu = ngonNgu,
                DienVien = dienVien,
                KhoiChieu = khoiChieu,
                MoTa = moTa,
                Poster = posterUrl,
                Banner = bannerUrl,
                Trailer = trailer,
            };

            var created = await _phimService.CreateAsync(phim);

            var chiTietList = maLoaiPhims.Select(loaiId => new ChiTietLoaiPhim
            {
                MaPhim = created.MaPhim,
                MaLoaiPhim = loaiId
            }).ToList();

            await _phimService.AddLoaiPhimToPhimAsync(chiTietList);

            var mapped = _mapper.Map<PhimDTO>(created);
            return CreatedAtAction(nameof(GetById), new { id = mapped.MaPhim },
                new { status = true, message = "Tạo phim thành công", data = mapped });
        }

        [Authorize(Roles = "Employee,Admin")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePhimRequest request)
        {
            if (id != request.MaPhim)
                return BadRequest(new { status = false, message = "Mã phim không khớp", data = (object?)null });

            var model = _mapper.Map<Phim>(request);
            var success = await _phimService.UpdateAsync(model);

            if (success) return NoContent();
            return NotFound(new { status = false, message = "Không tìm thấy phim để cập nhật", data = (object?)null });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _phimService.DeleteAsync(id);
            if (success) return NoContent();
            return NotFound(new { status = false, message = "Không tìm thấy phim để xoá", data = (object?)null });
        }
    }
}
