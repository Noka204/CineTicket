using System.Security.Cryptography;
using System.Text;
using CineTicket.Data;
using CineTicket.Models.Momo;
using CineTicket.Models.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using System.Globalization;
using CineTicket.Repositories.Interfaces;
using CineTicket.Services.Interfaces;

namespace CineTicket.Services
{
    public class MomoService : IMomoService
    {
        private readonly IOptions<MomoOptionModel> _options;
        private readonly CineTicketDbContext _context;
        private readonly ILogger<MomoService> _logger;

        public MomoService(
            IOptions<MomoOptionModel> options,
            CineTicketDbContext context,
            ILogger<MomoService> logger,
            IVeService veService,
            IHoaDonRepository hoaDonRepository)
        {
            _options = options;
            _context = context;
            _logger  = logger;

        }

        public async Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model)
        {
            if (string.IsNullOrWhiteSpace(model.OrderId))
                throw new ArgumentException("orderId is required");

            var requestId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            var momoOrderId = $"{model.OrderId}-{requestId}"; // <-- unique cho MoMo
            var amountStr = model.Amount.ToString(CultureInfo.InvariantCulture);

            var extraObj = new { internalOrderId = model.OrderId };
            var extraData = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(extraObj)));

            model.OrderInfo = $"Khách hàng: {model.FullName}. Nội dung: {model.OrderInfo}";

            var raw = $"accessKey={_options.Value.AccessKey}"
                    + $"&amount={amountStr}"
                    + $"&extraData={extraData}"
                    + $"&ipnUrl={_options.Value.NotifyUrl}"
                    + $"&orderId={momoOrderId}"                 // <-- dùng momoOrderId
                    + $"&orderInfo={model.OrderInfo}"
                    + $"&partnerCode={_options.Value.PartnerCode}"
                    + $"&redirectUrl={_options.Value.ReturnUrl}"
                    + $"&requestId={requestId}"
                    + $"&requestType={_options.Value.RequestType}";

            var signature = ComputeHmacSha256(raw, _options.Value.SecretKey);

            var body = new
            {
                partnerCode = _options.Value.PartnerCode,
                accessKey = _options.Value.AccessKey,
                requestId = requestId,
                amount = amountStr,                         // <-- string Invariant
                orderId = momoOrderId,                       // <-- unique
                orderInfo = model.OrderInfo,
                redirectUrl = _options.Value.ReturnUrl,
                ipnUrl = _options.Value.NotifyUrl,
                extraData = extraData,
                requestType = _options.Value.RequestType,
                signature = signature,
                lang = "vi"
            };

            var client  = new RestClient(_options.Value.MomoApiUrl);
            var request = new RestRequest { Method = Method.Post };
            request.AddHeader("Content-Type", "application/json; charset=UTF-8");
            request.AddStringBody(JsonConvert.SerializeObject(body), DataFormat.Json);

            try
            {
                using var scope = _logger.BeginScope(new Dictionary<string, object?>
                {
                    ["momo_url"] = _options.Value.MomoApiUrl,
                    ["requestId"] = requestId,
                    ["orderId"]   = model.OrderId,
                    ["amount"]    = model.Amount
                });

                _logger.LogInformation("MOMO CREATE -> sending request: {Body}", JsonConvert.SerializeObject(body));

                var response = await client.ExecuteAsync(request);

                _logger.LogInformation("MOMO CREATE <- response HTTP {Status} IsSuccess={IsSuccess}",
                    (int)response.StatusCode, response.IsSuccessful);
                if (!string.IsNullOrEmpty(response.ErrorMessage))
                    _logger.LogWarning("MOMO CREATE errorMessage: {Err}", response.ErrorMessage);

                if (!string.IsNullOrEmpty(response.Content))
                    _logger.LogInformation("MOMO CREATE raw: {Raw}", response.Content);
                else
                    _logger.LogWarning("MOMO CREATE empty body");

                var parsed = string.IsNullOrEmpty(response.Content)
                    ? new MomoCreatePaymentResponseModel()
                    : JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(response.Content)
                      ?? new MomoCreatePaymentResponseModel();

                return parsed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MOMO CREATE exception for orderId={OrderId}", model.OrderId);
                // ném lại để controller bắt và trả lỗi chung
                throw;
            }
        }
        public MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection)
        {
            var amount = collection.First(s => s.Key == "amount").Value;
            var orderInfo = collection.First(s => s.Key == "orderInfo").Value;
            var orderId = collection.First(s => s.Key == "orderId").Value;

            return new MomoExecuteResponseModel()
            {
                Amount = amount,
                OrderId = orderId,
                OrderInfo = orderInfo
            };
        }
        public async Task<bool> ConfirmByQueryAsync(MomoNotifyRequestModel model)
        {
            // 1) /query xác nhận
            var raw = $"accessKey={_options.Value.AccessKey}"
                    + $"&orderId={model.orderId}"
                    + $"&partnerCode={_options.Value.PartnerCode}"
                    + $"&requestId={model.requestId}";
            var signature = ComputeHmacSha256(raw, _options.Value.SecretKey);

            var body = new
            {
                partnerCode = _options.Value.PartnerCode,
                accessKey = _options.Value.AccessKey,
                requestId = model.requestId,
                orderId = model.orderId,
                signature = signature,
                lang = "vi"
            };

            var client = new RestClient("https://test-payment.momo.vn/v2/gateway/api/query");
            var req = new RestRequest { Method = Method.Post };
            req.AddHeader("Content-Type", "application/json; charset=UTF-8");
            req.AddStringBody(JsonConvert.SerializeObject(body), DataFormat.Json);

            var res = await client.ExecuteAsync(req);
            if (string.IsNullOrEmpty(res.Content)) return false;

            dynamic o = JsonConvert.DeserializeObject(res.Content)!;
            if ((int)o.resultCode != 0) return false;

            // 2) Lấy MaHd
            string? internalOrderId = null;
            try
            {
                if (!string.IsNullOrEmpty(model.extraData))
                {
                    var json = Encoding.UTF8.GetString(Convert.FromBase64String(model.extraData));
                    internalOrderId = JsonConvert.DeserializeObject<dynamic>(json)?.internalOrderId?.ToString();
                }
            }
            catch { /* ignore */ }

            var rawKey = internalOrderId ?? model.orderId?.Split('-')[0];
            if (!int.TryParse(rawKey, out var maHd)) return false;

            // 3) Transaction + 1 SaveChanges
            using var tx = await _context.Database.BeginTransactionAsync();

            var hoaDon = await _context.HoaDons
                .Include(h => h.ChiTietHoaDons)
                    .ThenInclude(ct => ct.MaVeNavigation)
                .FirstOrDefaultAsync(h => h.MaHd == maHd);

            if (hoaDon == null) return false;

            if (!string.Equals(hoaDon.TrangThai, "Đã thanh toán", StringComparison.OrdinalIgnoreCase))
            {
                hoaDon.TrangThai = "Đã thanh toán";
                hoaDon.HinhThucThanhToan = "Ví Momo";

                if (decimal.TryParse((string)o.amount, NumberStyles.Any, CultureInfo.InvariantCulture, out var amt))
                    hoaDon.TongTien = amt;

                foreach (var ct in hoaDon.ChiTietHoaDons)
                {
                    var ve = ct.MaVeNavigation ?? (ct.MaVe.HasValue ? await _context.Ves.FindAsync(ct.MaVe.Value) : null);
                    if (ve != null)
                    {
                        ve.TrangThai = "DaDat";
                        ve.NgayDat = DateTime.Now;
                        ve.ThoiGianTamGiu = null;
                    }
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
            }
            else
            {
                await tx.RollbackAsync(); // Không thay đổi gì
            }

            return true;
        }

        // Verify chữ ký IPN (thứ tự field theo tài liệu IPN v2)
        public bool VerifySignature(MomoNotifyRequestModel body)
        {
            // Dùng accessKey từ cấu hình (MoMo không bắt buộc gửi accessKey trong IPN)
            string accessKeyCfg = _options.Value.AccessKey ?? string.Empty;

            string S(string? s) => s ?? string.Empty;
            string Ns(long v) => v.ToString(CultureInfo.InvariantCulture);
            string Ni(int v) => v.ToString(CultureInfo.InvariantCulture);

            // Thứ tự đúng theo IPN v2
            var rawData =
                "accessKey=" + accessKeyCfg +
                "&amount=" + Ns(body.amount) +
                "&extraData=" + S(body.extraData) +
                "&message=" + S(body.message) +
                "&orderId=" + S(body.orderId) +
                "&orderInfo=" + S(body.orderInfo) +
                "&orderType=" + S(body.orderType) +
                "&partnerCode=" + S(body.partnerCode) +
                "&payType=" + S(body.payType) +
                "&requestId=" + S(body.requestId) +
                "&responseTime=" + Ns(body.responseTime) +
                "&resultCode=" + Ni(body.resultCode) +
                "&transId=" + Ns(body.transId);

            var mySig = ComputeHmacSha256(rawData, _options.Value.SecretKey);

            // logging để so sánh khi cần
            _logger.LogInformation("IPN verify raw: {raw}", rawData);
            _logger.LogInformation("IPN mySig: {my} | momoSig: {mo}", mySig, body.signature);

            return string.Equals(mySig, body.signature, StringComparison.OrdinalIgnoreCase);
        }
        private string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(messageBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
