using CineTicket.Data;
using CineTicket.DTOs.Ve;
using CineTicket.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Services
{
    public class SeatHoldExpiryWorker : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly IHubContext<SeatHub> _hub;

        public SeatHoldExpiryWorker(IServiceProvider sp, IHubContext<SeatHub> hub)
        { _sp = sp; _hub = hub; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<CineTicketDbContext>();
                    var now = DateTime.Now;

                    var expired = await db.Ves
                        .Where(v => v.TrangThai == "TamGiu" && v.ThoiGianTamGiu != null && v.ThoiGianTamGiu <= now)
                        .ToListAsync(stoppingToken);

                    if (expired.Count > 0)
                    {
                        foreach (var v in expired)
                        {
                            v.TrangThai = "Trong";
                            v.ThoiGianTamGiu = null;
                        }
                        await db.SaveChangesAsync(stoppingToken);

                        foreach (var v in expired)
                        {
                            var payload = new SeatUpdatePayload
                            {
                                MaSuat = v.MaSuat ?? 0,
                                MaGhe = v.MaGhe ?? 0,
                                TrangThai = "Trong",
                                Reason = "expire"
                            };
                            await _hub.Clients.Group(SeatHub.GroupName(payload.MaSuat))
                                              .SendAsync("SeatUpdated", payload, stoppingToken);
                        }
                    }
                }
                catch { /* TODO: log */ }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

}
