namespace CineTicket.DTOs.Ve
{
    public class GiuGheResponse
    {
        public int MaVe { get; set; }              
        public int MaGhe { get; set; }          
        public int MaSuat { get; set; }  
        public string TrangThai { get; set; } = "TamGiu";
        public string? ThoiGianHetHan { get; set; }
    }

}
