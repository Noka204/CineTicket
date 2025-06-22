using AutoMapper;
using CineTicket.DTOs;
using CineTicket.DTOs.Auth;
using CineTicket.DTOs.ChiTietHoaDon;
using CineTicket.DTOs.HoaDon;
using CineTicket.DTOs.LoaiPhim;
using CineTicket.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CineTicket.MappingProfiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Phim
            CreateMap<Phim, PhimDTO>()
                .ForMember(dest => dest.TenLoaiPhim, opt => opt.MapFrom(src => src.MaLoaiPhimNavigation != null ? src.MaLoaiPhimNavigation.TenLoaiPhim : null));
            CreateMap<CreatePhimRequest, Phim>()
                .ForMember(dest => dest.MaPhim, opt => opt.Ignore());

            CreateMap<UpdatePhimRequest, Phim>()
                .ForMember(dest => dest.MaLoaiPhimNavigation, opt => opt.Ignore()); // Không map Navigation property

            // SuatChieu
            CreateMap<SuatChieu, SuatChieuDTO>()
                .ForMember(dest => dest.TenPhim, opt => opt.MapFrom(src => src.MaPhimNavigation.TenPhim))
                .ForMember(dest => dest.TenPhong, opt => opt.MapFrom(src => src.MaPhongNavigation.TenPhong))
                .ForMember(dest => dest.GioChieu, opt => opt.MapFrom(src =>
                    src.ThoiGianBatDau.HasValue ? src.ThoiGianBatDau.Value.ToString("HH:mm") : null));

            CreateMap<CreateSuatChieuRequest, SuatChieu>();
            CreateMap<UpdateSuatChieuRequest, SuatChieu>();

            // PhongChieu
            CreateMap<PhongChieu, PhongChieuDTO>();
            CreateMap<CreatePhongChieuRequest, PhongChieu>();
            CreateMap<UpdatePhongChieuRequest, PhongChieu>();

            // Ve
            CreateMap<Ve, VeDTO>()
                .ForMember(dest => dest.SoGhe, opt => opt.MapFrom(src => src.MaGheNavigation.SoGhe))
                .ForMember(dest => dest.ThoiGianBatDau, opt => opt.MapFrom(src => src.MaSuatNavigation.ThoiGianBatDau));
            CreateMap<CreateVeRequest, Ve>();
            CreateMap<UpdateVeRequest, Ve>();

            //Ghe
            CreateMap<Ghe, GheDTO>()
                .ForMember(dest => dest.TenPhong, opt => opt.MapFrom(src => src.MaPhongNavigation.TenPhong));

            CreateMap<CreateGheRequest, Ghe>();
            CreateMap<UpdateGheRequest, Ghe>();

            // LoaiPhim
            CreateMap<LoaiPhim, LoaiPhimDTO>();
            CreateMap<CreateLoaiPhimRequest, LoaiPhim>();
            CreateMap<UpdateLoaiPhimRequest, LoaiPhim>();

            // Acccount
            CreateMap<RegisterUserRequest, ApplicationUser>();
            CreateMap<ApplicationUser, ApplicationUserDTO>();

            CreateMap<UpdateUserInfoRequest, ApplicationUser>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()); // Tránh update ID

            // BapNuoc
            CreateMap<BapNuoc, BapNuocDTO>().ReverseMap();
            CreateMap<BapNuocCreateDTO, BapNuoc>();
            CreateMap<BapNuocUpdateDTO, BapNuoc>();

            // HoaDon
            CreateMap<CreateHoaDonDTO, HoaDon>();
            CreateMap<UpdateHoaDonDTO, HoaDon>();
            CreateMap<HoaDon, HoaDonDTO>();

            CreateMap<CreateHoaDonDTO, HoaDon>();
            CreateMap<CreateChiTietHoaDonDTO, ChiTietHoaDon>();
            CreateMap<UpdateHoaDonDTO, HoaDon>();
            CreateMap<HoaDon, HoaDonDTO>();
            CreateMap<ChiTietHoaDon, ChiTietHoaDonDTO>();
        }
    }
}
