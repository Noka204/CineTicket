using System.Globalization;
using System.Resources;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using CineTicket.Localization; // chứa class marker: public class SharedResource {}

namespace CineTicket.Api.Controllers
{
    [ApiController]
    [Route("api/i18n")]
    public class I18nController : ControllerBase
    {
        private static readonly Dictionary<string, string> AliasToCulture = new(StringComparer.OrdinalIgnoreCase)
        {
            ["vi"] = "vi-VN",
            ["en"] = "en-US",
            ["fr"] = "fr-FR"
        };

        private static readonly string[] Keys =
         {
            // ===== Header =====
            "Nav_Home","Nav_Movies","Nav_Theaters","Nav_Contact","Nav_Admin","Nav_Language",

            // ===== Footer =====
            "Footer_Desc","Footer_Contact_Title","Footer_Hours_Title","Footer_Address",
            "Footer_Phone_Label","Footer_Email_Label","Footer_Hours_Week","Footer_Hours_Holiday",
            "Footer_Copyright_Tpl",

            // ===== Home / Hero =====
            "Page_Title","Hero_Title","Hero_Subtitle","Hero_CTA_ViewMovies",
            "Section_Movies_Title",

            // ===== Movies list =====
            "Loading_Movies","NoData_Movies","Error_Movies","Movie_DefaultTitle",

            // ===== Common buttons/links =====
            "Button_BuyTicket","Link_ViewDetail",

            // ===== Booking filters & showtimes =====
            "Label_ShowDate","Label_City","Label_Cinema","Label_RoomShow",
            // (Bổ sung label dùng chung)
            "Label_Room","Label_Showtime",
            "Loading_Cities","NoData_Cities","Loading_Cinemas","NoData_Cinemas",
            "Info_MissingFilters","Loading_Showtimes","NoData_Showtimes","Error_Showtimes",
            "Cinema_Number_Tpl","Room_Number_Tpl",

            // ===== Movie Detail page =====
            "Detail_PageTitle","Detail_Director","Detail_Cast","Detail_Language",
            "Detail_Duration","Detail_Duration_MinUnit","Detail_ReleaseDate",
            "Detail_Genres","Detail_Rating","Detail_Synopsis",
            "Detail_ViewTrailer","Detail_BookNow","Detail_NoTrailer",
            "Detail_NoData","Detail_Loading",

            // ===== Trailer modal =====
            "Trailer_Title","Trailer_Close","Trailer_Loading","Trailer_Error",

            // ===== Seat Selection (Step 2) =====
            "Seat_Page_Title",
            "Seat_Legend_Free",
            "Seat_Legend_Selected",
            "Seat_Legend_BookedHeld",
            "Seat_Screen_Label",
            "Loading_Seats",
            "Seat_Error_NoSeats",
            "Seat_Error_Server",
            "Lang_English",
            "Lang_French",
            "Lang_Vietnamese",

            // ===== Seat Summary / Totals =====
            "Sum_Subtotal_Label",
            "Sum_Total_Label",
            "Seat_Count_Label",
            "Seat_ItemBreakdown_Format",
            "Breakdown_Title",
            "Seat_Pending_Label",

            // ===== Buttons (flow) =====
            "Btn_Next_Snacks",
            // (Bổ sung trạng thái/nút dùng chung)
            "Btn_Retry",
            "Loading_Processing",

            // ===== Messages (alerts / validations) =====
            "Msg_LoginRequired",
            "Msg_MissingShowtime",
            "Msg_ShowtimeNotFound",
            "Msg_MaxSeatsReached",
            "Msg_NoLonelySeat_Generic",
            "Msg_NoLonelySeat_Specific",
            "Msg_SeatTaken",
            "Msg_HoldFailed_Generic",
            // (Bổ sung cho Checkout/Payment – giữ Msg_... cũ, thêm key mới không đè)
            "Auth_Login_Required",
            "Error_Missing_Session",
            "Error_Missing_ShowtimeInfo",
            "Error_NoInvoiceId",
            "Error_InvalidAmount",
            "Error_Booking_Generic",

            // ===== Snacks/Combos (Step 3) – CHUẨN HÓA BN_* =====
            "BN_Page_Title",        // <title>
            "BN_Heading",           // H2 tiêu đề trang
            "BN_Loading_Combos",    // "Đang tải combo…"
            "BN_Empty_List",        // "Hiện chưa có combo."
            "BN_Fetch_Failed",      // "Không tải được combo."
            "BN_Summary_None",      // "Chưa chọn combo nào"
            "BN_Total_Label",       // "Tổng combo"
            "BN_Price_Label",       // "Giá:"
            "BN_Next_Checkout",     // "Tiếp — Thanh toán"
            "BN_Alert_NoSeats",     // "Bạn chưa chọn ghế."
            "BN_ItemLine_Format",   // "{name} × {qty}"
            "Qty_Decrease_Aria",    // ARIA: Giảm
            "Qty_Increase_Aria",    // ARIA: Tăng

            // ===== Checkout (Step 4) =====
            "Checkout_Page_Title",              // <title> trang thanh toán
            "Checkout_Heading",                 // H2 "💳 Thanh toán"
            "Checkout_PayNow_Button",           // "🎟 Thanh toán ngay"
            "Checkout_SelectedTickets_Heading", // "Vé đã chọn"
            "Checkout_Combo_Heading",           // "🍿 Combo"
            "Checkout_TotalToPay_Label",        // "Tổng thanh toán"

            // ===== Payment / MoMo =====
            "Payment_OrderInfo_Tpl",   // "Thanh toán vé phim — HĐ {orderId}"
            "User_Default_FullName",   // "Khách hàng"

            // ===== Profile / Account =====
            "Profile_Page_Title",
            "Profile_Heading",
            "Loading_Profile",
            "Profile_Update_Success",
            "Profile_Update_Failed",
            "Error_Profile_Load",

            // Labels (Profile)
            "Label_FullName",
            "Label_UserName",
            "Label_Email",
            "Label_Phone",
            "Label_Address",

            // Buttons (Profile)
            "Btn_Edit",
            "Btn_Save",
            "Btn_Cancel",

            // ===== Nav (Account area) =====
            "Nav_Profile",
            "Nav_BookingHistory",
            "Nav_Promotions",

            // ===== Booking History (Account) =====
            "History_Page_Title",
            "History_Heading",
            "History_Loading",
            "History_Empty",
            "History_Load_Failed",
            "History_InvoiceLine_Tpl",   // "Mã HĐ: #{id} • Ngày lập: {date}"
            "History_Showtime_Line",     // "Giờ chiếu: {time}"
            "History_TotalPaid_Label",   // "Tổng thanh toán"

            // Pager (History)
            "Pager_Prev",
            "Pager_Next",
            "Pager_PageInfo_Tpl",        // "Trang {p}/{n}"

            // ===== Promo (Account) =====
            "Promo_Page_Title",
            "Promo_Heading"
        };




        private readonly IStringLocalizer _L;

        // Dùng factory + baseName tường minh để khớp đúng resource set
        public I18nController(IStringLocalizerFactory factory)
        {
            var asmName = typeof(SharedResource).Assembly.GetName().Name!;      // "CineTicket"
            var baseName = "CineTicket.Localization.SharedResource";              // => Embedded: CineTicket.Localization.SharedResource.resources
            _L = factory.Create(baseName, asmName);
        }

        [HttpGet]
        public IActionResult Get([FromQuery] string? lang = null)
        {
            // Nhận alias ?lang=vi/en/fr hoặc culture đầy đủ
            if (!string.IsNullOrWhiteSpace(lang))
            {
                var pick = AliasToCulture.TryGetValue(lang, out var mapped) ? mapped : lang;
                try
                {
                    var ci = CultureInfo.GetCultureInfo(pick);
                    CultureInfo.CurrentCulture = ci;
                    CultureInfo.CurrentUICulture = ci;
                }
                catch { /* ignore */ }
            }

            // Probe log
            var probe = _L["Hero_Title"];
            Console.WriteLine($"[i18n] UICulture={CultureInfo.CurrentUICulture.Name}, NotFound={probe.ResourceNotFound}, Value={probe.Value}");

            // Xuất dict key -> value
            var dict = Keys.ToDictionary(k => k, k => _L[k].Value);
            Response.Headers["Cache-Control"] = "public, max-age=300";
            Response.Headers["Content-Language"] = CultureInfo.CurrentUICulture.Name;
            return Ok(dict);
        }

        // ===================== DIAG =====================
        // GET /api/i18n/diag
        [HttpGet("diag")]
        public IActionResult Diag()
        {
            var asm = typeof(SharedResource).Assembly;
            var asmName = asm.GetName().Name!;
            var baseDir = AppContext.BaseDirectory;
            var baseName = "CineTicket.Localization.SharedResource";
            var rm = new ResourceManager(baseName, asm);
            var cultures = new[] { "vi-VN", "vi", "en-US", "fr-FR" };

            // 1) Liệt kê embedded resources
            var embedded = asm.GetManifestResourceNames();

            // 2) Kiểm tra satellite assemblies tồn tại + file .resources.dll
            var satellites = new List<object>();
            foreach (var c in new[] { "vi-VN", "en-US", "fr-FR" })
            {
                var info = new Dictionary<string, object?>();
                info["culture"] = c;

                try
                {
                    var satAsm = asm.GetSatelliteAssembly(new CultureInfo(c));
                    info["satelliteLoaded"] = satAsm?.FullName;
                }
                catch (Exception ex)
                {
                    info["satelliteLoaded"] = $"MISSING: {ex.GetType().Name}: {ex.Message}";
                }

                var filePath = Path.Combine(baseDir, c, $"{asmName}.resources.dll");
                info["satelliteFile"] = new { path = filePath, exists = System.IO.File.Exists(filePath) };

                satellites.Add(info);
            }

            // 3) ResourceManager truy vấn “thô”
            var rmProbe = cultures.ToDictionary(
                c => c,
                c =>
                {
                    try { return rm.GetString("Hero_Title", CultureInfo.GetCultureInfo(c)) ?? "<null>"; }
                    catch (Exception ex) { return $"<err:{ex.GetType().Name}>"; }
                });

            // 4) IStringLocalizer (đổi UICulture tạm thời)
            var localizerProbe = new List<object>();
            var saved = CultureInfo.CurrentUICulture;
            try
            {
                foreach (var c in new[] { "vi-VN", "en-US", "fr-FR" })
                {
                    CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(c);
                    var v = _L["Hero_Title"];
                    localizerProbe.Add(new
                    {
                        culture = c,
                        notFound = v.ResourceNotFound,
                        value = v.Value
                    });
                }
            }
            finally
            {
                CultureInfo.CurrentUICulture = saved;
            }

            // 5) Trả JSON tổng hợp
            var result = new
            {
                baseDir,
                assembly = asm.FullName,
                baseName,
                embedded,
                satellites,
                rmProbe,
                localizerProbe,
                currentUICulture = CultureInfo.CurrentUICulture.Name
            };

            return Ok(result);
        }
    }
}
