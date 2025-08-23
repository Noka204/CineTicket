using CineTicket.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineTicket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Employee")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _service;

        public DashboardController(IDashboardService service)
        {
            _service = service;
        }

        // NEW: doanh thu 30 ngày gần nhất
        [HttpGet("doanh-thu-30-ngay")]
        public async Task<IActionResult> GetDoanhThu30NgayGanNhat([FromQuery] bool onlyPaid = true)
        {
            var data = await _service.GetDoanhThu30NgayGanNhatAsync(onlyPaid);

            var today = DateTime.Today;
            var start = today.AddDays(-29);

            // Map về dict để fill ngày trống
            var dict = data.ToDictionary(d => d.Ngay.Date, d => d.TongTien);

            var result = Enumerable.Range(0, 30)
                .Select(offset =>
                {
                    var date = start.AddDays(offset);
                    dict.TryGetValue(date, out var sum);
                    return new
                    {
                        Ngay = date.ToString("yyyy-MM-dd"),
                        TongTien = sum
                    };
                });

            return Ok(new { status = true, data = result });
        }
    }
}
