using AutoMapper;
using CineTicket.DTOs.HoaDon;
using CineTicket.Models;
using CineTicket.Repositories.Interfaces;
using CineTicket.Services;
using CineTicket.Services.Interfaces;

namespace CineTicket.Services.Implementations
{
    public class HoaDonService : IHoaDonService
    {
        private readonly IHoaDonRepository _hoaDonRepo;
        private readonly IMapper _mapper;

        public HoaDonService(IHoaDonRepository hoaDonRepo, IMapper mapper)
        {
            _hoaDonRepo = hoaDonRepo;
            _mapper = mapper;
        }

        public async Task<HoaDon> CreateWithDetailsAsync(CreateHoaDonDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.HinhThucThanhToan))
                throw new Exception("Hóa đơn chưa được thanh toán");

            var hoaDon = new HoaDon
            {
                NgayLap = dto.NgayLap,
                TongTien = dto.TongTien,
                TrangThai = "Đã đặt",
                HinhThucThanhToan = dto.HinhThucThanhToan,
                ChiTietHoaDons = dto.ChiTietHoaDons.Select(x => new ChiTietHoaDon
                {
                    MaVe = x.MaVe,
                    MaBn = x.MaBn,
                    SoLuong = x.SoLuong
                }).ToList()
            };


            return await _hoaDonRepo.CreateAsync(hoaDon);
        }


        public async Task<HoaDon?> GetByIdAsync(int id) => await _hoaDonRepo.GetByIdAsync(id);

        public async Task<IEnumerable<HoaDon>> GetAllAsync() => await _hoaDonRepo.GetAllAsync();

        public async Task<bool> UpdateAsync(UpdateHoaDonDTO dto)
        {
            var entity = _mapper.Map<HoaDon>(dto);
            return await _hoaDonRepo.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int id) => await _hoaDonRepo.DeleteAsync(id);
    }

}