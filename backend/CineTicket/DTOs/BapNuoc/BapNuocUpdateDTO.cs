namespace CineTicket.DTOs
{
    public class BapNuocUpdateDTO
    {
        public int MaBn { get; set; }
        public string TenBn { get; set; } = null!;
        public decimal Gia { get; set; }
        public string? MoTa { get; set; }
    }
}
