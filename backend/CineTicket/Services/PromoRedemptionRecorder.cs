using CineTicket.Models;
using CineTicket.Repositories.Interfaces;
using CineTicket.Utils;

namespace CineTicket.Services
{
    public class PromoRedemptionRecorder
    {
        private readonly IHoaDonKhuyenMaiRepository _hdkmRepo;

        public PromoRedemptionRecorder(IHoaDonKhuyenMaiRepository hdkmRepo)
        {
            _hdkmRepo = hdkmRepo;
        }

        public async Task RecordForCommonAsync(
            int maHd,
            int khuyenMaiId,
            string? userId,
            DiscountKind loaiGiam,
            decimal mucGiam,
            decimal giaTriGiam,
            string? codeText
        )
        {
            var rec = new HoaDonKhuyenMai
            {
                MaHd = maHd,
                KhuyenMaiId = khuyenMaiId,
                KhuyenMaiCodeId = null,
                Code = string.IsNullOrWhiteSpace(codeText) ? null : PromoTextUtil.NormalizeCode(codeText),
                UserId = userId,
                LoaiGiam = loaiGiam,
                MucGiam = mucGiam,
                GiaTriGiam = giaTriGiam
            };
            await _hdkmRepo.AddAsync(rec);
        }

        public async Task RecordForPersonalCodeAsync(
            int maHd,
            int khuyenMaiCodeId,
            DiscountKind loaiGiam,
            decimal mucGiam,
            decimal giaTriGiam,
            string codeText
        )
        {
            var rec = new HoaDonKhuyenMai
            {
                MaHd = maHd,
                KhuyenMaiId = null,
                KhuyenMaiCodeId = khuyenMaiCodeId,
                Code = PromoTextUtil.NormalizeCode(codeText),
                UserId = null, // đích danh đã nằm ở AssignedToUserId của KhuyenMaiCode
                LoaiGiam = loaiGiam,
                MucGiam = mucGiam,
                GiaTriGiam = giaTriGiam
            };
            await _hdkmRepo.AddAsync(rec);
        }
    }
}
