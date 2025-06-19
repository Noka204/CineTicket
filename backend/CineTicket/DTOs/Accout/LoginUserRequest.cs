namespace CineTicket.DTOs.Auth
{
    public class LoginUserRequest
    {
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
