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

        [HttpGet("doanh-thu-ngay")]
        public async Task<IActionResult> GetDoanhThuNgay([FromQuery] int year, [FromQuery] int month)
        {
            if (!ModelState.IsValid || year < 1 || month < 1 || month > 12)
            {
                return BadRequest(new { status = false, message = "Invalid year or month." });
            }

            var data = await _service.GetDoanhThuTheoNgayTrongThang(year, month);
            if (data == null)
            {
                return Ok(new { status = true, data = new List<object>() });
            }

            int daysInMonth = DateTime.DaysInMonth(year, month);
            var doanhThuDict = data.ToDictionary(d => d.Ngay.Date, d => d.TongTien);

            var result = Enumerable.Range(1, daysInMonth)
                .Select(day =>
                {
                    var date = new DateTime(year, month, day);
                    doanhThuDict.TryGetValue(date, out decimal tongTien);
                    return new
                    {
                        Ngay = date.ToString("yyyy-MM-dd"),
                        TongTien = tongTien
                    };
                });

            return Ok(new { status = true, data = result });
        }
    }
}
