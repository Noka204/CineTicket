namespace CineTicket.DTOs.Ve
{
    public class DatNhieuGheResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<int> DanhSachMaVe { get; set; } = new();
    }
}

