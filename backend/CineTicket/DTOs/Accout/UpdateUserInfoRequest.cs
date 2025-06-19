namespace CineTicket.DTOs.Auth
{
    public class UpdateUserInfoRequest
    {
        public string Id { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Address { get; set; } = null!;
    } 
}
