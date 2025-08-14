namespace CineTicket.DTOs.Ve
{
    public sealed record SeatUpdatePayload
    {
        public int MaSuat { get; init; }
        public int MaGhe { get; init; }
        public string TrangThai { get; init; } = default!; 
        public string? ThoiGianHetHan { get; init; }    
        public string Reason { get; init; } = "";    
    }

}
