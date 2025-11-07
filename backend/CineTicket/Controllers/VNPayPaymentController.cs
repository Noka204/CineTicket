using CineTicket.Models.VNPay;
using CineTicket.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities; // <-- để dùng QueryHelpers
using System.Globalization;

namespace VNPayApi.VNPay
{
    [ApiController]
    [Route("api/payments/vnpay")]
    public class VNPayPaymentController : ControllerBase
    {
        private readonly IVnPayService _vnPay;
        private readonly IConfiguration _cfg;

        public VNPayPaymentController(IVnPayService vnPay, IConfiguration cfg)
        {
            _vnPay = vnPay;
            _cfg = cfg;
        }

        [HttpPost("create")]
        public IActionResult CreatePaymentUrlVnpay([FromBody] PaymentInformationModel model)
        {
            var url = _vnPay.CreatePaymentUrl(model, HttpContext);
            return Ok(new { url });
        }

        // Sau khi user được VNPay redirect về đây:
        // → xác nhận & cập nhật DB → Redirect sang FE index với query báo trạng thái
        [HttpGet("return")]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            var (ok, maHd, amount, orderRef, message) = await _vnPay.ConfirmAndSettleAsync(Request.Query);

            var fe = _cfg["Frontend:ReturnUrl"]
                     ?? "http://127.0.0.1:5500/frontend/pages/Phim/index.html";

            var qs = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["pay"] = ok ? "success" : "fail", // FE đọc để hiện thông báo
                ["gw"] = "vnpay",
                ["maHd"] = maHd?.ToString(),
                ["amount"] = amount?.ToString(CultureInfo.InvariantCulture),
                ["ref"] = orderRef
            }
            .Where(kv => !string.IsNullOrEmpty(kv.Value))
            .ToDictionary(kv => kv.Key, kv => kv.Value!);

            var dest = QueryHelpers.AddQueryString(fe, qs);
            return Redirect(dest);
        }

        // IPN: vẫn xử lý settle DB như thường (không redirect, chỉ 200/400)
        [HttpGet("ipn")]
        [HttpPost("ipn")]
        public async Task<IActionResult> Ipn()
        {
            var (ok, maHd, amount, orderRef, message) = await _vnPay.ConfirmAndSettleAsync(Request.Query);
            if (!ok)
                return BadRequest(new { status = false, message, data = new { orderId = orderRef, amount, maHd } });

            return Ok(new { status = true, message = "OK", data = new { orderId = orderRef, amount, maHd } });
        }
    }
}
