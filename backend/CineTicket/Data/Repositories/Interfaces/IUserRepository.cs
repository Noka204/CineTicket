using CineTicket.Models;
using Microsoft.AspNetCore.Identity;

namespace CineTicket.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetByIdAsync(string id);
        Task<ApplicationUser?> GetByUserNameAsync(string username);
        Task<ApplicationUser?> GetByEmailAsync(string email);
        Task<IdentityResult> RegisterAsync(ApplicationUser user, string password);
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
        Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
        Task<bool> ResetPasswordAsync(ApplicationUser user, string token, string newPassword);
        Task<bool> UpdateUserInfoAsync(ApplicationUser user);
    }
}