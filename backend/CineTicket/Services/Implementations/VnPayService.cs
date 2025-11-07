using System.Text.RegularExpressions;
using CineTicket.Data;
using CineTicket.Models.VNPay;
using CineTicket.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CineTicket.Services.Implementations
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _cfg;
        private readonly CineTicketDbContext _db;
        private readonly ILogger<VnPayService> _logger;
        private readonly MailService _mail; // dùng chung như MoMo

        public VnPayService(IConfiguration configuration,
                            CineTicketDbContext db,
                            ILogger<VnPayService> logger,
                            MailService mail)
        {
            _cfg = configuration;
            _db = db;
            _logger = logger;
            _mail = mail;
        }

        public string CreatePaymentUrl(PaymentInformationModel model, HttpContext context)
        {
            var tzId = _cfg["TimeZoneId"] ?? "SE Asia Standard Time";
            var tz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);

            // Gợi ý: nếu model có OrderId => nên dùng làm vnp_TxnRef để map maHd trực tiếp.
            var tick = DateTime.UtcNow.Ticks.ToString();

            var pay = new VnPayLibrary();
            var urlCallBack = _cfg["Vnpay:PaymentBackReturnUrl"] ?? string.Empty;

            pay.AddRequestData("vnp_Version", _cfg["Vnpay:Version"]);
            pay.AddRequestData("vnp_Command", _cfg["Vnpay:Command"]);
            pay.AddRequestData("vnp_TmnCode", _cfg["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Amount", ((long)Math.Round(model.Amount * 100)).ToString());
            pay.AddRequestData("vnp_CreateDate", now.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _cfg["Vnpay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", VnPayLibrary.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", model.Locale ?? _cfg["Vnpay:Locale"]);
            pay.AddRequestData("vnp_OrderInfo", string.IsNullOrWhiteSpace(model.OrderDescription) ? $"{model.Name} {model.Amount}" : model.OrderDescription);
            pay.AddRequestData("vnp_OrderType", string.IsNullOrWhiteSpace(model.OrderType) ? "other" : model.OrderType);
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", tick); // Nếu có OrderId: nên set = OrderId để map nhanh

            if (!string.IsNullOrWhiteSpace(model.BankCode))
                pay.AddRequestData("vnp_BankCode", model.BankCode);
            if (model.ExpireMinutes is > 0)
                pay.AddRequestData("vnp_ExpireDate", now.AddMinutes(model.ExpireMinutes.Value).ToString("yyyyMMddHHmmss"));

            var url = pay.CreateRequestUrl(_cfg["Vnpay:BaseUrl"]!, _cfg["Vnpay:HashSecret"]!);
            return url;
        }

        public PaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            var pay = new VnPayLibrary();
            return pay.GetFullResponseData(collections, _cfg["Vnpay:HashSecret"]!);
        }

        // === NEW: xác nhận + cập nhật DB (dùng cho cả return và ipn) ===
        public async Task<(bool ok, int? maHd, decimal? amount, string? orderRef, string message)>
            ConfirmAndSettleAsync(IQueryCollection query)
        {
            // 1) Verify & parse qua VnPayLibrary
            var resp = PaymentExecute(query);
            var responseCode = resp.VnPayResponseCode; // đa số lib trả thế này. Nếu lib khác tên, giữ thêm fallback dưới.
            var success = resp.Success || string.Equals(responseCode, "00", StringComparison.OrdinalIgnoreCase);

            // Fallback lấy thẳng từ query để đảm bảo chuẩn VNPay
            var q = query.ToDictionary(k => k.Key, v => v.Value.ToString());
            if (!success)
            {
                if (q.TryGetValue("vnp_ResponseCode", out var rc))
                    success = string.Equals(rc, "00", StringComparison.OrdinalIgnoreCase);
            }

            // 2) Lấy amount (VNPay x100)
            decimal? amount = null;
            if (q.TryGetValue("vnp_Amount", out var amountStr) && long.TryParse(amountStr, out var a100))
                amount = a100 / 100m;

            // 3) Lấy orderRef (TxnRef)
            q.TryGetValue("vnp_TxnRef", out var orderRef);

            // 4) Trích maHd:
            //    - ưu tiên TxnRef nếu là số
            //    - nếu không, tìm trong OrderInfo: “HĐ <digits>” hoặc chuỗi số cuối
            int? maHd = null;
            if (!string.IsNullOrEmpty(orderRef) && int.TryParse(orderRef, out var idFromRef) && idFromRef > 0)
            {
                maHd = idFromRef;
            }
            else
            {
                if (q.TryGetValue("vnp_OrderInfo", out var info) && !string.IsNullOrWhiteSpace(info))
                {
                    // match HĐ 1234 hoặc chuỗi số dài cuối cùng
                    var m = Regex.Match(info, @"HĐ\D*(\d+)", RegexOptions.IgnoreCase);
                    if (m.Success && int.TryParse(m.Groups[1].Value, out var idFromInfo))
                        maHd = idFromInfo;
                    else
                    {
                        var m2 = Regex.Match(info, @"(\d{1,})\b");
                        if (m2.Success && int.TryParse(m2.Value, out var id2))
                            maHd = id2;
                    }
                }
            }

            if (!success)
                return (false, maHd, amount, orderRef, "VNPay response not successful");

            if (maHd is null || maHd <= 0)
                return (false, null, amount, orderRef, "Cannot resolve invoice id (maHd) from VNPay return");

            // 5) Cập nhật DB idempotent như MoMo
            try
            {
                using var tx = await _db.Database.BeginTransactionAsync();

                var hoaDon = await _db.HoaDons
                    .Include(h => h.ChiTietHoaDons)
                        .ThenInclude(ct => ct.MaVeNavigation)
                    .FirstOrDefaultAsync(h => h.MaHd == maHd.Value);

                if (hoaDon == null)
                {
                    await tx.RollbackAsync();
                    return (false, maHd, amount, orderRef, $"Invoice not found | maHd={maHd}");
                }

                if (!string.Equals(hoaDon.TrangThai, "Đã thanh toán", StringComparison.OrdinalIgnoreCase))
                {
                    hoaDon.TrangThai = "Đã thanh toán";
                    hoaDon.HinhThucThanhToan = "VNPAY";
                    if (amount.HasValue && amount.Value > 0) hoaDon.TongTien = amount.Value;

                    foreach (var ct in hoaDon.ChiTietHoaDons)
                    {
                        var ve = ct.MaVeNavigation ?? (ct.MaVe.HasValue ? await _db.Ves.FindAsync(ct.MaVe.Value) : null);
                        if (ve != null)
                        {
                            ve.TrangThai = "DaDat";
                            ve.NgayDat = DateTime.Now;
                            ve.ThoiGianTamGiu = null;
                        }
                    }

                    await _db.SaveChangesAsync();
                    await tx.CommitAsync();

                    // email hóa đơn
                    await _mail.SendInvoiceEmailAsync(maHd.Value);
                    return (true, maHd, amount, orderRef, "VNPay settled successfully");
                }
                else
                {
                    await tx.RollbackAsync();
                    _logger.LogInformation("Invoice already paid, skip update | maHd={MaHd}", maHd);
                    return (true, maHd, amount, orderRef, "Invoice already paid");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VNPay settle exception | maHd={MaHd}", maHd);
                return (false, maHd, amount, orderRef, "Exception during settlement");
            }
        }
    }
}
