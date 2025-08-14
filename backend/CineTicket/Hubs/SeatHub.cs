// SeatHub.cs
using CineTicket.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace CineTicket.Hubs
{
    public class SeatHub : Hub
    {
        private readonly CineTicketDbContext _db;
        public SeatHub(CineTicketDbContext db) { _db = db; }

        public static string GroupName(int maSuat) => $"suat-{maSuat}";

        public async Task JoinShowtime(int maSuat)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(maSuat));
            var now = DateTime.Now;

            // Lấy snapshot nhanh
            var snapshot = await (from g in _db.Ghes
                                  join s in _db.SuatChieus on g.MaPhong equals s.MaPhong
                                  where s.MaSuat == maSuat
                                  join v in _db.Ves.Where(x => x.MaSuat == maSuat)
                                       on g.MaGhe equals v.MaGhe into gj
                                  from v in gj.DefaultIfEmpty()
                                  select new
                                  {
                                      maGhe = g.MaGhe,
                                      soGhe = g.SoGhe,
                                      trangThai = v == null ? "Trong" :
                                          (v.TrangThai == "DaDat" ? "DaDat" :
                                           (v.TrangThai == "TamGiu" && v.ThoiGianTamGiu != null && v.ThoiGianTamGiu > now ? "TamGiu" : "Trong"))
                                  }).ToListAsync();
            await Clients.Caller.SendAsync("SeatsSnapshot", snapshot);
        }

        public Task LeaveShowtime(int maSuat)
            => Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(maSuat));
    }
}
