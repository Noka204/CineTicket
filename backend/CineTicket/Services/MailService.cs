using System.Diagnostics;
using CineTicket.Data;
using CineTicket.Models;
using MailKit;                            // <-- thêm
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;

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
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("📧 Begin SendInvoiceEmailAsync | maHd={MaHd}", maHd);

        try
        {
            var hoaDon = await _context.HoaDons
                .Include(h => h.ChiTietHoaDons).ThenInclude(ct => ct.MaVeNavigation).ThenInclude(ve => ve.MaGheNavigation)
                .Include(h => h.ChiTietHoaDons).ThenInclude(ct => ct.MaVeNavigation).ThenInclude(ve => ve.MaSuatNavigation).ThenInclude(s => s.MaPhimNavigation)
                .Include(h => h.ChiTietHoaDons).ThenInclude(ct => ct.MaBnNavigation)
                .Include(h => h.ApplicationUser)
                .FirstOrDefaultAsync(h => h.MaHd == maHd);

            if (hoaDon == null || hoaDon.ApplicationUser == null)
            {
                _logger.LogWarning("📧 Skip: NotFound invoice or user | maHd={MaHd}", maHd);
                return;
            }

            // ---- Load config
            string fromEmail = _config["EmailSettings:SenderEmail"];
            string senderName = _config["EmailSettings:SenderName"];
            string smtpHost = _config["EmailSettings:Smtp:Host"];
            string smtpPortRaw = _config["EmailSettings:Smtp:SmtpPort"];
            string smtpUser = _config["EmailSettings:Smtp:User"];
            string smtpPass = _config["EmailSettings:Smtp:Pass"];
            bool useSsl = bool.Parse(_config["EmailSettings:Smtp:UseSsl"] ?? "true");
            int smtpPort = int.TryParse(smtpPortRaw, out var parsedPort) ? parsedPort : 587;

            bool enableProtoLog = bool.TryParse(_config["EmailSettings:Smtp:EnableProtocolLog"], out var v) && v;
            var protoPath = _config["EmailSettings:Smtp:ProtocolLogPath"] ?? "Logs/smtp-protocol.log";

            var toEmail = hoaDon.ApplicationUser.Email ?? "";
            var toName = hoaDon.ApplicationUser.FullName ?? hoaDon.ApplicationUser.UserName ?? "khách hàng";

            _logger.LogInformation("📧 SMTP cfg host={Host} port={Port} ssl={Ssl} user={User} protoLog={Proto} path={Path}",
                smtpHost, smtpPort, useSsl, smtpUser, enableProtoLog, protoPath);

            // ---- Compose
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(senderName, fromEmail));
            msg.To.Add(MailboxAddress.Parse(toEmail));
            msg.Subject = "[CineTicket] Cảm ơn quý khách đã đặt vé";
            msg.Body = new BodyBuilder { HtmlBody = BuildHtmlBody(hoaDon, toName) }.ToMessageBody();

            // ---- SMTP (có thể log protocol ra file)
            using var client = enableProtoLog
                ? new SmtpClient(new ProtocolLogger(protoPath))
                : new SmtpClient();

            // (Optional) log lỗi chứng chỉ nếu có
            client.ServerCertificateValidationCallback = (s, c, h, e) =>
            {
                _logger.LogDebug("📧 Cert: {Errors}", e);
                return true; // hoặc để mặc định nếu bạn không muốn bỏ qua
            };

            _logger.LogInformation("📧 Connecting…");
            await client.ConnectAsync(smtpHost, smtpPort, useSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
            _logger.LogInformation("📧 Connected. Capabilities AUTH={Auth}", string.Join(",", client.AuthenticationMechanisms));

            _logger.LogInformation("📧 Authenticating as {User}…", smtpUser);
            await client.AuthenticateAsync(smtpUser, smtpPass);
            _logger.LogInformation("📧 Auth OK.");

            _logger.LogInformation("📧 Sending to {To} | subject='{Subject}'", toEmail, msg.Subject);
            await client.SendAsync(msg);
            _logger.LogInformation("📧 Send OK (maHd={MaHd}).", maHd);

            await client.DisconnectAsync(true);
            _logger.LogInformation("📧 Disconnected. Elapsed={Elapsed}ms", sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ SendInvoiceEmailAsync FAILED | maHd={MaHd}", maHd);
            throw;
        }
        finally
        {
            sw.Stop();
        }
    }

    public async Task SendOtpEmailAsync(string toEmail, string otp)
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("📧 Begin SendOtpEmail | to={To}", toEmail);

        try
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
            _logger.LogInformation("📧 Connect {Host}:{Port} ssl={Ssl}", smtpHost, smtpPort, useSsl);
            await client.ConnectAsync(smtpHost, smtpPort, useSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);

            _logger.LogInformation("📧 Auth as {User}", smtpUser);
            await client.AuthenticateAsync(smtpUser, smtpPass);

            await client.SendAsync(message);
            _logger.LogInformation("📧 OTP sent OK to {To}", toEmail);

            await client.DisconnectAsync(true);
            _logger.LogInformation("📧 Done. Elapsed={Ms}ms", sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ SendOtpEmail FAILED | to={To}", toEmail);
            throw;
        }
        finally
        {
            sw.Stop();
        }
    }
    private static string BuildHtmlBody(HoaDon hd, string toName)
    {
        string cur(decimal? v) => (v ?? 0m).ToString("N0");
        string dt(DateTime? d) => d?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";
        string tm(string? t) => TimeSpan.TryParse(t, out var ts) ? ts.ToString(@"hh\:mm") : "N/A";

        // Vé
        var veParts = new List<string>();
        foreach (var ct in hd.ChiTietHoaDons.Where(x => x.MaVeNavigation != null))
        {
            var ve = ct.MaVeNavigation!;
            var soGhe = ve.MaGheNavigation?.SoGhe ?? "N/A";
            var suat = ve.MaSuatNavigation;
            var phim = suat?.MaPhimNavigation?.TenPhim ?? "N/A";
            var ngay = suat?.NgayChieu?.ToString("dd/MM/yyyy") ?? "N/A";
            var gio = tm(suat?.GioChieu);
            var donGia = cur(ct.DonGia);

            veParts.Add($"<li>Phim: <b>{phim}</b> — Ghế: <b>{soGhe}</b> — Suất: {ngay} {gio} — Giá: <b>{donGia}đ</b></li>");
        }
        var veHtml = veParts.Count > 0
            ? $"<h4>🎟️ Vé</h4><ul style='padding-left:20px'>{string.Join("", veParts)}</ul>"
            : "";

        // Bắp nước
        var bnParts = new List<string>();
        foreach (var g in hd.ChiTietHoaDons
                            .Where(x => x.MaBnNavigation != null)
                            .GroupBy(x => new { x.MaBnNavigation!.TenBn, x.MaBnNavigation!.Gia }))
        {
            var ten = g.Key.TenBn ?? "Bắp/Nước";
            var gia = cur(g.Key.Gia);
            var sl = g.Sum(x => x.SoLuong);
            bnParts.Add($"<li>{ten} — SL: <b>{sl}</b> — Giá: <b>{gia}đ</b></li>");
        }
        var bnHtml = bnParts.Count > 0
            ? $"<h4>🥤 Bắp nước</h4><ul style='padding-left:20px'>{string.Join("", bnParts)}</ul>"
            : "";

        return $@"
<html>
  <body style='font-family:Arial, sans-serif; background:#f7f7f7; padding:24px'>
    <div style='max-width:640px; margin:auto; background:#fff; border-radius:10px; padding:24px; box-shadow:0 4px 18px rgba(0,0,0,.06)'>
      <h2 style='margin-top:0'>Cảm ơn {toName} đã đặt vé tại CineTicket!</h2>
      <p>Thông tin hóa đơn của bạn:</p>
      <ul style='list-style:none; padding-left:0'>
        <li>🧾 Mã hóa đơn: <b>#{hd.MaHd}</b></li>
        <li>📅 Ngày lập: {dt(hd.NgayLap)}</li>
        <li>💳 Thanh toán: {hd.HinhThucThanhToan ?? "N/A"}</li>
        <li>💰 Tổng tiền: <b>{cur(hd.TongTien)}đ</b></li>
      </ul>
      <hr/>
      {veHtml}
      {bnHtml}
      <hr/>
      <p style='color:#666; font-size:13px'>Chúc bạn xem phim vui vẻ! 🎬</p>
      <p style='color:#aaa; font-size:12px'>Email này được gửi tự động. Vui lòng không trả lời.</p>
    </div>
  </body>
</html>";
    }
}
