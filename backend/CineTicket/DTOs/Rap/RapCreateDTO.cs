namespace CineTicket.DTOs.Rap
{
    public class RapCreateDTO
    {
        public string TenRap { get; set; } = "";
        public string? DiaChi { get; set; }
        public string? ThanhPho { get; set; }
        public bool? HoatDong { get; set; } = true;
    }
}
