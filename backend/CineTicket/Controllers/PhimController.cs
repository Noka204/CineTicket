using AutoMapper;
using CineTicket.DTOs;
using CineTicket.DTOs.LoaiPhim;
using CineTicket.Models;
using CineTicket.Services;
using CineTicket.Services.Interfaces;
using Google.Cloud.Translate.V3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CineTicket.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhimController : ControllerBase
    {
        private readonly IPhimService _phimService;
        private readonly IMapper _mapper;
        private readonly FileService _fileService;
        private readonly ILibreTranslateService _trans; // << inject đúng interface

        public PhimController(IPhimService phimService, IMapper mapper, FileService fileService, ILibreTranslateService trans)
        {
            _phimService = phimService;
            _mapper = mapper;
            _fileService = fileService;
            _trans = trans;
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
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None, Duration = 0)]
        public async Task<IActionResult> GetById(int id, [FromQuery] string? lang, CancellationToken ct)
        {
            // --- chặn cache ở mọi tầng ---
            Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
            Response.Headers.Pragma = "no-cache";
            Response.Headers["Vary"] = "Accept-Language, Cookie";

            // --- resolve ngôn ngữ: query > header > cookie > vi ---
            var resolved = NormalizeLang(lang)
                ?? NormalizeLang(Request.Headers["Accept-Language"].ToString())
                ?? NormalizeLang(Request.Cookies["lang"])
                ?? "vi";
            Response.Headers["X-Lang-Resolved"] = resolved;

            // --- lấy dữ liệu ---
            var phim = await _phimService.Query()
                .AsNoTracking()
                .Include(p => p.ChiTietLoaiPhims)
                    .ThenInclude(ct => ct.LoaiPhim)
                .FirstOrDefaultAsync(p => p.MaPhim == id, ct);

            if (phim == null)
                return NotFound(new { status = false, message = "Không tìm thấy phim", data = (object?)null });

            var dto = _mapper.Map<PhimDTO>(phim);

            // --- dịch nếu không phải vi ---
            if (resolved != "vi")
            {
                try
                {
                    var trans = HttpContext.RequestServices.GetRequiredService<ILibreTranslateService>();

                    // Gom các chuỗi cần dịch vào một batch để nhanh & đồng bộ
                    var toTrans = new List<string>
            {
                dto.MoTa ?? string.Empty,
                dto.NgonNgu ?? string.Empty,
            };

                    var loai = dto.LoaiPhims?.ToList() ?? new List<LoaiPhimDTO>();
                    toTrans.AddRange(loai.Select(x => x.TenLoaiPhim ?? string.Empty));

                    var outs = await trans.TranslateManyAsync(
                        texts: toTrans,
                        target: resolved,
                        source: "vi",
                        ct: ct
                    );

                    int i = 0;
                    dto.MoTa = outs[i++];
                    dto.NgonNgu = outs[i++];
                    foreach (var g in loai) g.TenLoaiPhim = outs[i++];
                }
                catch (OperationCanceledException) { throw; } // FE abort -> ném ra cho middleware
                catch (Exception ex)
                {

                }
            }

            return Ok(new { status = true, message = "Lấy phim thành công", data = dto });
        }

        private static string? NormalizeLang(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            s = s.Trim().ToLowerInvariant();
            // "en-US,en;q=0.9" -> "en"
            if (s.Contains(',')) s = s.Split(',')[0];
            if (s.Contains(';')) s = s.Split(';')[0];
            if (s.Contains('-')) s = s.Split('-')[0];
            return s is "vi" or "en" or "fr" ? s : null;
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

        // Update bằng DTO cũ (JSON)
        [Authorize(Roles = "Employee,Admin")]
        [HttpPut("update/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> UpdateJson(int id, [FromBody] UpdatePhimRequest req)
        {
            if (id != req.MaPhim)
                return BadRequest(new { status = false, message = "Mã phim không khớp", data = (object?)null });

            var phim = await _phimService.GetByIdAsync(id);
            if (phim == null)
                return NotFound(new { status = false, message = "Không tìm thấy phim", data = (object?)null });

            // Map các trường từ DTO cũ
            phim.TenPhim = req.TenPhim ?? phim.TenPhim;
            phim.ThoiLuong = req.ThoiLuong ?? phim.ThoiLuong;
            phim.NgonNgu = req.NgonNgu ?? phim.NgonNgu;
            phim.DienVien = req.DienVien ?? phim.DienVien;
            phim.KhoiChieu = req.KhoiChieu ?? phim.KhoiChieu;
            phim.DaoDien = req.DaoDien ?? phim.DaoDien;
            phim.MoTa = req.MoTa ?? phim.MoTa;
            phim.Trailer = req.Trailer ?? phim.Trailer;
            // Poster/Banner trong DTO cũ là string path → KHÔNG xử lý file ở đây
            // IsHot cũng giữ nguyên nếu bạn có rule riêng cập nhật

            var ok = await _phimService.UpdateAsync(phim);

            // Cập nhật loại phim nếu gửi LoaiPhims (từ DTO cũ)
            if (req.LoaiPhims is { Count: > 0 })
            {
                var ids = req.LoaiPhims.Select(x => x.MaLoaiPhim).Distinct().ToList();
                await _phimService.UpdateLoaiPhimOfPhimAsync(id, ids);
            }
            else if (req.MaLoaiPhim.HasValue)
            {
                await _phimService.UpdateLoaiPhimOfPhimAsync(id, new List<int> { req.MaLoaiPhim.Value });
            }

            if (!ok) return StatusCode(500, new { status = false, message = "Cập nhật thất bại" });
            return Ok(new { status = true, message = "Cập nhật thành công" });
        }
        [Authorize(Roles = "Employee,Admin")]
        [HttpPut("update-poster/{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdatePoster(int id, [FromForm] IFormFile poster, [FromServices] FileService file)
        {
            var phim = await _phimService.GetByIdAsync(id);
            if (phim == null) return NotFound(new { status = false, message = "Không tìm thấy phim" });
            if (poster is null || poster.Length == 0) return BadRequest(new { status = false, message = "Poster rỗng" });

            var url = await file.SaveFileAsync(poster, "Poster");
            phim.Poster = url;
            var ok = await _phimService.UpdateAsync(phim);
            if (!ok) return StatusCode(500, new { status = false, message = "Cập nhật poster thất bại" });
            return Ok(new { status = true, message = "Đã cập nhật poster", data = url });
        }

        [Authorize(Roles = "Employee,Admin")]
        [HttpPut("update-banner/{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateBanner(int id, [FromForm] IFormFile banner, [FromServices] FileService file)
        {
            var phim = await _phimService.GetByIdAsync(id);
            if (phim == null) return NotFound(new { status = false, message = "Không tìm thấy phim" });
            if (banner is null || banner.Length == 0) return BadRequest(new { status = false, message = "Banner rỗng" });

            var url = await file.SaveFileAsync(banner, "Banner");
            phim.Banner = url;
            var ok = await _phimService.UpdateAsync(phim);
            if (!ok) return StatusCode(500, new { status = false, message = "Cập nhật banner thất bại" });
            return Ok(new { status = true, message = "Đã cập nhật banner", data = url });
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
