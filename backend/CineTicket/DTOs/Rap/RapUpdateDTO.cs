namespace CineTicket.DTOs.Rap
{
    public class RapUpdateDTO
    {
        public int MaRap { get; set; }
        public string TenRap { get; set; } = "";
        public string? DiaChi { get; set; }
        public string? ThanhPho { get; set; }
        public bool? HoatDong { get; set; } = true;
    }
}
