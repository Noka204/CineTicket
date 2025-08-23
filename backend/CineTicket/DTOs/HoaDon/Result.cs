namespace CineTicket.DTOs.HoaDon
{
    public class Result
    {
        public bool IsValid { get; set; }
        public string? Message { get; set; }
        public List<int> OffendingSeats { get; set; } = new List<int>();
    }
}
