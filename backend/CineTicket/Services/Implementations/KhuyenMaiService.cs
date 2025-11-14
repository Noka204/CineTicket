using CineTicket.DTOs.KhuyenMai;
using CineTicket.Models;
using CineTicket.Repositories.Interfaces;
using CineTicket.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using CineTicket.Data;

namespace CineTicket.Services.Implementations
{
    public class KhuyenMaiService : IKhuyenMaiService
    {
        private readonly IKhuyenMaiRepository _repo;
        private readonly CineTicketDbContext _db;

        public KhuyenMaiService(IKhuyenMaiRepository repo, CineTicketDbContext db)
        {
            _repo = repo;
            _db = db;
        }

        public async Task<List<KhuyenMaiOutDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(x => new KhuyenMaiOutDto
            {
                Id = x.Id,
                Ten = x.Ten,
                IsActive = x.IsActive,
                LoaiGiam = x.LoaiGiam,
                MucGiam = x.MucGiam,
                BatDau = x.BatDau,
                KetThuc = x.KetThuc,
                SoCode = x.Codes?.Count ?? 0
            }).ToList();
        }

        public async Task<KhuyenMaiOutDto?> GetAsync(int id)
        {
            var x = await _repo.GetAsync(id);
            if (x is null) return null;
            return new KhuyenMaiOutDto
            {
                Id = x.Id,
                Ten = x.Ten,
                IsActive = x.IsActive,
                LoaiGiam = x.LoaiGiam,
                MucGiam = x.MucGiam,
                BatDau = x.BatDau,
                KetThuc = x.KetThuc,
                SoCode = x.Codes?.Count ?? 0
            };
        }

        public async Task<KhuyenMaiOutDto> CreateAsync(KhuyenMaiCreateDto dto)
        {
            var km = new KhuyenMai
            {
                Ten = dto.Ten,
                IsActive = dto.IsActive,
                LoaiGiam = dto.LoaiGiam,
                MucGiam = dto.MucGiam,
                BatDau = dto.BatDau,
                KetThuc = dto.KetThuc
            };
            await _repo.AddAsync(km);
            return new KhuyenMaiOutDto
            {
                Id = km.Id,
                Ten = km.Ten,
                IsActive = km.IsActive,
                LoaiGiam = km.LoaiGiam,
                MucGiam = km.MucGiam,
                BatDau = km.BatDau,
                KetThuc = km.KetThuc,
                SoCode = km.Codes?.Count ?? 0
            };
        }

        public async Task<KhuyenMaiOutDto?> UpdateAsync(KhuyenMaiUpdateDto dto)
        {
            var km = await _repo.GetAsync(dto.Id);
            if (km is null) return null;
            km.Ten = dto.Ten;
            km.IsActive = dto.IsActive;
            km.LoaiGiam = dto.LoaiGiam;
            km.MucGiam = dto.MucGiam;
            km.BatDau = dto.BatDau;
            km.KetThuc = dto.KetThuc;
            await _repo.UpdateAsync(km);

            return new KhuyenMaiOutDto
            {
                Id = km.Id,
                Ten = km.Ten,
                IsActive = km.IsActive,
                LoaiGiam = km.LoaiGiam,
                MucGiam = km.MucGiam,
                BatDau = km.BatDau,
                KetThuc = km.KetThuc,
                SoCode = km.Codes?.Count ?? 0
            };
        }

        public Task<bool> DeleteAsync(int id) => _repo.DeleteAsync(id);


        public async Task<ApplyCouponResult> ValidateAsync(string code, decimal? amount, string? userId)
        {
            var baseAmount = amount ?? 0m;         // OK vì amount là nullable
            code = (code ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(code))
                return ApplyCouponResult.Fail("Ma khong hop le.");

            // 1) Ưu tiên KhuyenMaiCode
            var kmc = await _db.KhuyenMaiCodes
                .Include(c => c.KhuyenMai)
                .FirstOrDefaultAsync(c => c.Code == code);

            KhuyenMai? km = kmc?.KhuyenMai;
            int? kmcId = kmc?.Id;

            if (kmc != null)
            {
                if (kmc.RedeemedAt.HasValue)
                    return ApplyCouponResult.Fail("Ma da duoc su dung.");
                if (!string.IsNullOrEmpty(kmc.AssignedToUserId) &&
                    !string.Equals(kmc.AssignedToUserId, userId, StringComparison.Ordinal))
                    return ApplyCouponResult.Fail("Ma khong thuoc tai khoan nay.");
            }

            // 2) Fallback: public code theo Ten
            if (km == null)
                km = await _db.KhuyenMais.FirstOrDefaultAsync(x => x.Ten == code);
            if (km == null)
                return ApplyCouponResult.Fail("Khong tim thay khuyen mai.");

            // 3) Trang thai & thoi gian
            var now = DateTime.Now;
            if (!km.IsActive) return ApplyCouponResult.Fail("Khuyen mai dang tat.");
            if (km.BatDau.HasValue && now < km.BatDau.Value) return ApplyCouponResult.Fail("Khuyen mai chua bat dau.");
            if (km.KetThuc.HasValue && now > km.KetThuc.Value) return ApplyCouponResult.Fail("Khuyen mai da het han.");

            // 4) Tinh giam (cho baseAmount; GET/validate = 0 -> discount=0)
            decimal muc = km.MucGiam;  // MucGiam la decimal non-nullable
            decimal discount = km.LoaiGiam == DiscountKind.Percent
                ? Math.Round(baseAmount * (muc / 100m), 0, MidpointRounding.AwayFromZero)
                : muc;

            if (discount < 0) discount = 0;
            if (discount > baseAmount) discount = baseAmount;

            var data = new ApplyCouponResponseDto
            {
                Code = code,
                // Nếu DTO của bạn để string:
                LoaiGiam = km.LoaiGiam.ToString(),   // "Percent" | "Amount"
                MucGiam = km.MucGiam,
                Discount = discount,
                FinalAmount = baseAmount - discount,
                KhuyenMaiId = km.Id,
                KhuyenMaiCodeId = kmcId,
                Ten = km.Ten,
                BatDau = km.BatDau,
                KetThuc = km.KetThuc,
                Note = kmc != null ? "Ap theo ma phat hanh" : "Ap theo ten khuyen mai (public)"
            };

            return ApplyCouponResult.Ok(data);
        }

    }
}
