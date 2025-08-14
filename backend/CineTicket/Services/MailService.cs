using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using CineTicket.Data;
using CineTicket.Models;
using System.Net;

public class MailService
{
    private readonly CineTicketDbContext _context;
    private readonly IConfiguration _config;
    private readonly ILogger<MailService> _logger;

    public MailService(CineTicketDbContext context, IConfiguration config, ILogger<MailService> logger)
    {
        _context = context;
        _config = config;
        _logger = logger;
    }

    public async Task SendInvoiceEmailAsync(int maHd)
    {
        try
        {
            Console.WriteLine($"🔍 [MailService] Bắt đầu gửi hóa đơn #{maHd}");

            var hoaDon = await _context.HoaDons
                .Include(h => h.ChiTietHoaDons)
                    .ThenInclude(ct => ct.MaVeNavigation)
                        .ThenInclude(ve => ve.MaGheNavigation)
                .Include(h => h.ChiTietHoaDons)
                    .ThenInclude(ct => ct.MaVeNavigation)
                        .ThenInclude(ve => ve.MaSuatNavigation)
                            .ThenInclude(s => s.MaPhimNavigation)
                .Include(h => h.ChiTietHoaDons)
                    .ThenInclude(ct => ct.MaBnNavigation)
                .Include(h => h.ApplicationUser)
                .FirstOrDefaultAsync(h => h.MaHd == maHd);

            if (hoaDon == null || hoaDon.ApplicationUser == null)
            {
                _logger.LogWarning($"❌ Không tìm thấy hóa đơn hoặc người dùng cho mã #{maHd}");
                return;
            }

            string fromEmail = _config["EmailSettings:SenderEmail"];
            string senderName = _config["EmailSettings:SenderName"];
            string smtpHost = _config["EmailSettings:Smtp:Host"];
            string smtpPortRaw = _config["EmailSettings:Smtp:SmtpPort"];
            string smtpUser = _config["EmailSettings:Smtp:User"];
            string smtpPass = _config["EmailSettings:Smtp:Pass"];
            bool useSsl = bool.Parse(_config["EmailSettings:Smtp:UseSsl"] ?? "true");
            int smtpPort = int.TryParse(smtpPortRaw, out var parsedPort) ? parsedPort : 587;

            string email = hoaDon.ApplicationUser.Email ?? "";
            string userName = hoaDon.ApplicationUser.FullName ?? hoaDon.ApplicationUser.UserName ?? "khách hàng";

            Console.WriteLine($"📤 Gửi tới: {email}, Người nhận: {userName}");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, fromEmail));
            message.To.Add(MailboxAddress.Parse(email)); // ✅ Không gửi tới chính mình nữa
            message.Subject = $"[CineTicket] Cảm ơn quý khách đặt đặt vé tại CineTicket";
            message.Body = new BodyBuilder
            {
                HtmlBody = BuildHtmlBody(hoaDon, userName)
            }.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(smtpHost, smtpPort, useSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
            await smtp.AuthenticateAsync(smtpUser, smtpPass);
            await smtp.SendAsync(message);

            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Lỗi khi gửi email hóa đơn");
            Console.WriteLine("❌ Exception: " + ex.Message);
            if (ex.InnerException != null)
            {
                Console.WriteLine("⛔ Inner: " + ex.InnerException.Message);
            }
            throw;
        }
    }
    public async Task SendOtpEmailAsync(string toEmail, string otp)
    {
        var emailSettings = _config.GetSection("EmailSettings");
        var smtpSettings = emailSettings.GetSection("Smtp");

        var fromEmail = emailSettings["SenderEmail"];
        var fromName = emailSettings["SenderName"];
        var smtpHost = smtpSettings["Host"];
        var smtpPort = int.Parse(smtpSettings["SmtpPort"]);
        var useSsl = bool.Parse(smtpSettings["UseSsl"]);
        var smtpUser = smtpSettings["User"];
        var smtpPass = smtpSettings["Pass"];

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = "Mã OTP đặt lại mật khẩu";

        message.Body = new TextPart("html")
        {
            Text = $"<p>Xin chào,</p><p>Mã OTP của bạn là: <b>{otp}</b></p><p>OTP có hiệu lực trong 5 phút.</p>"
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(smtpHost, smtpPort, useSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
        await client.AuthenticateAsync(smtpUser, smtpPass);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    private string BuildHtmlBody(HoaDon hoaDon, string userName)
    {
        string tongTienStr = hoaDon.TongTien?.ToString("N0") ?? "0";
        string ngayLapStr = hoaDon.NgayLap?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";

        // Tạo HTML cho chi tiết vé
        string veHtml = "";
        var veList = hoaDon.ChiTietHoaDons
            .Where(ct => ct.MaVeNavigation != null)
            .Select(ct => ct.MaVeNavigation)
            .ToList();

        if (veList.Any())
        {
            veHtml += "<h4>🎟️ Danh sách vé:</h4><ul style='padding-left:20px'>";
            foreach (var ve in veList)
            {
                string soGhe = ve.MaGheNavigation?.SoGhe ?? "N/A";
                string tenPhim = ve.MaSuatNavigation?.MaPhimNavigation?.TenPhim ?? "N/A";
                string ngayChieu = ve.MaSuatNavigation?.NgayChieu?.ToString("dd/MM/yyyy") ?? "N/A";
                string gioChieu = TimeSpan.TryParse(ve.MaSuatNavigation?.GioChieu, out var parsedGio)
                    ? parsedGio.ToString(@"hh\:mm")
                    : "N/A";

                string giaVe = ve.GiaVe?.ToString("N0") ?? "0";

                veHtml += $"<li><strong>Phim:</strong> {tenPhim} | <strong>Ghế:</strong> {soGhe} | <strong>Suất:</strong> {ngayChieu} {gioChieu} | <strong>Giá vé:</strong> {giaVe}đ</li>";
            }
            veHtml += "</ul>";
        }
        string bapNuocHtml = "";
        var bapNuocList = hoaDon.ChiTietHoaDons
            .Where(ct => ct.MaBnNavigation != null)
            .Select(ct => ct.MaBnNavigation)
            .GroupBy(bn => new { bn.TenBn, bn.Gia })
            .Select(g => new
            {
                Ten = g.Key.TenBn,
                Gia = g.Key.Gia,
                SoLuong = g.Count()
            })
            .ToList();
        if (bapNuocList.Any())
        {
            bapNuocHtml += "<h4>🥤 Danh sách bắp nước:</h4><ul style='padding-left:20px'>";
            foreach (var item in bapNuocList)
            {
                string gia = item.Gia?.ToString("N0") ?? "0";
                bapNuocHtml += $"<li><strong>{item.Ten}</strong> - Số lượng: {item.SoLuong} | Giá mỗi cái: {gia}đ</li>";
            }
            bapNuocHtml += "</ul>";
        }
        return $@"
        <html>
        <body style='font-family: Arial, sans-serif; background-color: #f9f9f9; padding: 20px;'>
            <div style='max-width: 600px; margin: auto; background: #fff; border-radius: 8px; padding: 20px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);'>
                <h2 style='color: #333;'>🎟 Cảm ơn bạn {userName} đã đặt vé tại CineTicket!</h2>
                <p style='font-size: 16px; color: #555;'>Thông tin đơn hàng:</p>
                <ul style='list-style: none; padding: 0; font-size: 15px; color: #444;'>
                    <li><strong>🧾 Mã hoá đơn:</strong> {hoaDon.MaHd}</li>
                    <li><strong>📅 Ngày lập:</strong> {ngayLapStr}</li>
                    <li><strong>💳 Hình thức thanh toán:</strong> {hoaDon.HinhThucThanhToan}</li>
                    <li><strong>💰 Tổng tiền:</strong> {tongTienStr}đ</li>
                </ul>
                <hr style='margin: 20px 0;'>

                {veHtml}
                {bapNuocHtml}

                <hr style='margin: 20px 0;'>
                <p style='font-size: 14px; color: #777;'>CineTicket chúc bạn có trải nghiệm tuyệt vời tại rạp! 🎬</p>
                <p style='font-size: 13px; color: #aaa;'>Email này được gửi tự động, vui lòng không trả lời lại.</p>
            </div>
        </body>
        </html>";
    }
}
