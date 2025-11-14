using CineTicket.Models;

namespace CineTicket.DTOs.KhuyenMai
{
    public class ApplyCouponRequestDto
    {
        public string Code { get; set; } = "";
        public decimal Amount { get; set; }   // tổng tiền hiện tại (để tính mức giảm)
    }

    public class ApplyCouponResponseDto
    {
        public string Code { get; set; } = "";
        public string LoaiGiam { get; set; } = "";
        public decimal MucGiam { get; set; }
        public decimal Discount { get; set; }
        public decimal FinalAmount { get; set; }

        public int KhuyenMaiId { get; set; }
        public int? KhuyenMaiCodeId { get; set; }

        // Các thông tin mô tả (nếu muốn hiển thị ở Validate GET)
        public string? Ten { get; set; }
        public DateTime? BatDau { get; set; }
        public DateTime? KetThuc { get; set; }
        public string? Note { get; set; }
    }

    public class ApplyCouponResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public ApplyCouponResponseDto? Data { get; set; }
        public static ApplyCouponResult Ok(ApplyCouponResponseDto d) =>
            new() { Success = true, Data = d };
        public static ApplyCouponResult Fail(string msg) =>
            new() { Success = false, Message = msg };
    }
}
