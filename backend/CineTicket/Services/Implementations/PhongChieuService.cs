using AutoMapper;
using CineTicket.DTOs;
using CineTicket.Models;
using CineTicket.Repositories.Interfaces;
using CineTicket.Services.Interfaces;

namespace CineTicket.Services.Implementations
{
    public class PhongChieuService : IPhongChieuService
    {
        private readonly IPhongChieuRepository _phongRepo;
        private readonly IGheRepository _gheRepo;
        private readonly IMapper _mapper;

        public PhongChieuService(IPhongChieuRepository phongRepo, IMapper mapper)
        {
            _phongRepo = phongRepo;
            _mapper = mapper;
        }

        public Task<IEnumerable<PhongChieu>> GetAllAsync()
        {
            return _phongRepo.GetAllAsync();
        }

        public Task<PhongChieu?> GetByIdAsync(int id)
        {
            return _phongRepo.GetByIdAsync(id);
        }

        public Task<PhongChieu> CreateAsync(PhongChieu phongChieu)
        {
            return _phongRepo.CreateAsync(phongChieu);
        }
        public async Task<PhongChieuDTO> CreateWithSeatsAsync(CreatePhongChieuRequest request)
        {
            // Map request thành entity
            var phong = _mapper.Map<PhongChieu>(request);

            // Tạo phòng
            var created = await _phongRepo.CreateAsync(phong);

            // Tạo ghế tự động
            int tongSoGhe = (int)phong.SoGhe;
            int gheMoiHang = 10;
            int soHang = (int)Math.Ceiling(tongSoGhe / (double)gheMoiHang);

            var danhSachGhe = new List<Ghe>();

            for (int hang = 0; hang < soHang; hang++)
            {
                string tenHang = ((char)('A' + hang)).ToString();
                for (int cot = 1; cot <= gheMoiHang && danhSachGhe.Count < tongSoGhe; cot++)
                {
                    var soGhe = $"{tenHang}{cot}";
                    var ghe = new Ghe
                    {
                        SoGhe = soGhe,
                        LoaiGhe = (tenHang == "A" || tenHang == "B") ? "Vip" : "Thường",
                        MaPhong = created.MaPhong
                    };
                    danhSachGhe.Add(ghe);
                }
            }

            // Lưu từng ghế
            foreach (var ghe in danhSachGhe)
            {
                await _gheRepo.CreateAsync(ghe);
            }

            return _mapper.Map<PhongChieuDTO>(created);
        }
        public Task<bool> UpdateAsync(PhongChieu phongChieu)
        {
            return _phongRepo.UpdateAsync(phongChieu);
        }

        public Task<bool> DeleteAsync(int id)
        {
            return _phongRepo.DeleteAsync(id);
        }
    }
}
