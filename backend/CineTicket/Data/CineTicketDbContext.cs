using System;
using System.Collections.Generic;
using CineTicket.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Data;

public partial class CineTicketDbContext : IdentityDbContext<ApplicationUser>
{
    public CineTicketDbContext(DbContextOptions<CineTicketDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BapNuoc> BapNuocs { get; set; }

    public virtual DbSet<ChiTietHoaDon> ChiTietHoaDons { get; set; }

    public virtual DbSet<Ghe> Ghes { get; set; }

    public virtual DbSet<HoaDon> HoaDons { get; set; }

    public virtual DbSet<LoaiPhim> LoaiPhims { get; set; }

    public virtual DbSet<Phim> Phims { get; set; }

    public virtual DbSet<PhongChieu> PhongChieus { get; set; }

    public virtual DbSet<SuatChieu> SuatChieus { get; set; }

    public virtual DbSet<Ve> Ves { get; set; }
    public virtual DbSet<Rap> Raps { get; set; }
    public virtual DbSet<KhuyenMai> KhuyenMais { get; set; }
    public virtual DbSet<KhuyenMaiCode> KhuyenMaiCodes { get; set; }
    public virtual DbSet<HoaDonKhuyenMai> HoaDonKhuyenMais { get; set; }

    public DbSet<ChiTietLoaiPhim> ChiTietLoaiPhims { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=NOKA\\SQLEXPRESS;Database=CineTicketDB;Integrated Security=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<BapNuoc>(entity =>
        {
            entity.HasKey(e => e.MaBn).HasName("PK__BapNuoc__272475AD32F5D274");

            entity.ToTable("BapNuoc");

            entity.Property(e => e.MaBn).HasColumnName("MaBN");
            entity.Property(e => e.Gia).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.TenBn)
                .HasMaxLength(100)
                .HasColumnName("TenBN");
        });

        modelBuilder.Entity<ChiTietHoaDon>(entity =>
        {
            entity.HasKey(e => e.MaCthd).HasName("PK__ChiTietH__1E4FA7715A64D0C4");

            entity.ToTable("ChiTietHoaDon");

            entity.Property(e => e.MaCthd).HasColumnName("MaCTHD");
            entity.Property(e => e.MaBn).HasColumnName("MaBN");
            entity.Property(e => e.MaHd).HasColumnName("MaHD");

            entity.HasOne(d => d.MaBnNavigation).WithMany(p => p.ChiTietHoaDons)
                .HasForeignKey(d => d.MaBn)
                .HasConstraintName("FK__ChiTietHoa__MaBN__4E88ABD4");

            entity.HasOne(d => d.MaHdNavigation).WithMany(p => p.ChiTietHoaDons)
                .HasForeignKey(d => d.MaHd)
                .HasConstraintName("FK__ChiTietHoa__MaHD__4CA06362");

            entity.HasOne(d => d.MaVeNavigation).WithMany(p => p.ChiTietHoaDons)
                .HasForeignKey(d => d.MaVe)
                .HasConstraintName("FK__ChiTietHoa__MaVe__4D94879B");
        });

        modelBuilder.Entity<Ghe>(entity =>
        {
            entity.HasKey(e => e.MaGhe).HasName("PK__Ghe__3CD3C67BFCA14E9D");

            entity.ToTable("Ghe");

            entity.Property(e => e.LoaiGhe).HasMaxLength(20);
            entity.Property(e => e.SoGhe).HasMaxLength(10);

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.Ghes)
                .HasForeignKey(d => d.MaPhong)
                .HasConstraintName("FK__Ghe__MaPhong__4222D4EF");
        });

        modelBuilder.Entity<HoaDon>(entity =>
        {
            entity.HasKey(h => h.MaHd)
                  .HasName("PK__HoaDon__2725A6E0DE25F710");

            entity.ToTable("HoaDon");
            entity.Property(h => h.MaHd)
                  .HasColumnName("MaHD")
                  .ValueGeneratedOnAdd();

            entity.Property(h => h.HinhThucThanhToan)
                  .HasMaxLength(50);

            entity.Property(h => h.NgayLap)
                  .HasColumnType("datetime");

            entity.Property(h => h.TongTien)
                  .HasColumnType("decimal(10, 2)");
        });


        modelBuilder.Entity<LoaiPhim>(entity =>
        {
            entity.HasKey(e => e.MaLoaiPhim).HasName("PK__LoaiPhim__9CA05BEFEB048A14");

            entity.ToTable("LoaiPhim");

            entity.Property(e => e.TenLoaiPhim).HasMaxLength(100);
        });

        modelBuilder.Entity<Phim>(entity =>
        {
            entity.HasKey(e => e.MaPhim).HasName("PK__Phim__4AC03DE3772452C4");

            entity.ToTable("Phim");

            entity.Property(e => e.DaoDien).HasMaxLength(100);
            entity.Property(e => e.Poster).HasMaxLength(255);
            entity.Property(e => e.TenPhim).HasMaxLength(200);

        });
        modelBuilder.Entity<ChiTietLoaiPhim>()
            .ToTable("ChiTietLoaiPhim")
            .HasKey(ct => new { ct.MaPhim, ct.MaLoaiPhim });

        modelBuilder.Entity<ChiTietLoaiPhim>()
            .HasOne(ct => ct.Phim)
            .WithMany(p => p.ChiTietLoaiPhims)
            .HasForeignKey(ct => ct.MaPhim);

        modelBuilder.Entity<ChiTietLoaiPhim>()
            .HasOne(ct => ct.LoaiPhim)
            .WithMany(l => l.ChiTietLoaiPhims)
            .HasForeignKey(ct => ct.MaLoaiPhim);



        modelBuilder.Entity<PhongChieu>(entity =>
        {
            entity.HasKey(e => e.MaPhong).HasName("PK__PhongChi__20BD5E5BD2124FD6");

            entity.ToTable("PhongChieu");

            entity.Property(e => e.TenPhong).HasMaxLength(50);
        });

        modelBuilder.Entity<SuatChieu>(entity =>
        {
            entity.HasKey(e => e.MaSuat).HasName("PK__SuatChie__A69D0241FFB40570");

            entity.ToTable("SuatChieu");

            entity.Property(e => e.ThoiGianBatDau).HasColumnType("datetime");
            entity.Property(e => e.ThoiGianKetThuc).HasColumnType("datetime");

            entity.HasOne(d => d.MaPhimNavigation).WithMany(p => p.SuatChieus)
                .HasForeignKey(d => d.MaPhim)
                .HasConstraintName("FK__SuatChieu__MaPhi__3E52440B");

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.SuatChieus)
                .HasForeignKey(d => d.MaPhong)
                .HasConstraintName("FK__SuatChieu__MaPho__3F466844");
        });

        modelBuilder.Entity<Ve>(entity =>
        {
            entity.HasKey(e => e.MaVe).HasName("PK__Ve__2725100F335089D2");

            entity.ToTable("Ve");

            entity.Property(e => e.GiaVe).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TrangThai).HasMaxLength(20);

            entity.HasOne(d => d.MaGheNavigation).WithMany(p => p.Ves)
                .HasForeignKey(d => d.MaGhe)
                .HasConstraintName("FK__Ve__MaGhe__44FF419A");

            entity.HasOne(d => d.MaSuatNavigation).WithMany(p => p.Ves)
                .HasForeignKey(d => d.MaSuat)
                .HasConstraintName("FK__Ve__MaSuat__45F365D3");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
