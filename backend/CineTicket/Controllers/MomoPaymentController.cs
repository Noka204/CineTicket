using System.Text;
using CineTicket.Data;
using CineTicket.Models.Momo;
using CineTicket.Models.Order;
using CineTicket.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace CineTicket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MomoPaymentController : ControllerBase
    {
        private readonly IMomoService _momoService;
        private readonly IConfiguration _config;
        private readonly CineTicketDbContext _context;

        public MomoPaymentController(IMomoService momoService, IConfiguration config, CineTicketDbContext context)
        {
            _momoService = momoService;
            _config = config;
            _context = context;
        }

        // Body: { "fullName": "...", "orderId": "21", "orderInfo": "Thanh toán vé phim", "amount": 85000 }
        [HttpPost("create")]
        public async Task<IActionResult> CreatePaymentUrl([FromBody] OrderInfoModel model)
        {
            var response = await _momoService.CreatePaymentAsync(model);

            if (string.IsNullOrEmpty(response?.PayUrl))
                return BadRequest(new { status = false, message = "Không tạo được URL thanh toán." });

            return Ok(new
            {
                status = true,
                message = "Tạo URL thanh toán thành công",
                payUrl = response.PayUrl,
                orderId = model.OrderId // GIỮ NGUYÊN orderId client gửi
            });
        }

        [HttpGet("callback")]
        public IActionResult PaymentCallBack()
        {
            var q = HttpContext.Request.Query;
            var orderId = q["orderId"].ToString();     // momoOrderId
            var amount = q["amount"].ToString();
            var extraData = q["extraData"].ToString();

            string? maHd = null;
            if (!string.IsNullOrEmpty(extraData))
            {
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(extraData));
                maHd = JsonConvert.DeserializeObject<dynamic>(json)?.internalOrderId?.ToString();
            }

            return Ok(new
            {
                status = true,
                message = "Thanh toán thành công",
                data = new { orderId, amount, maHd }      // nếu bạn cần dùng maHd ở FE
            });
        }
        [AllowAnonymous]
        [HttpPost("momo-notify")]
        public async Task<IActionResult> MomoNotify([FromBody] MomoNotifyRequestModel model)
        {
            if (!_momoService.VerifySignature(model))
                return BadRequest("Invalid signature");

            if (model.resultCode == 0)  // số
            {
                var ok = await _momoService.ConfirmByQueryAsync(model);
                if (!ok) return BadRequest("Cannot confirm order");
            }
            return Ok();
        }
    }
}
