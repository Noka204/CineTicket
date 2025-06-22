namespace CineTicket.DTOs
{
    public class BapNuocCreateDTO
    {
        public string TenBn { get; set; } = null!;
        public decimal Gia { get; set; }
        public string? MoTa { get; set; }
    }
}
