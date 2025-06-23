using CineTicket.Models;
using Microsoft.AspNetCore.Identity;

namespace CineTicket.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApplicationUser?> GetByIdAsync(string id);
        Task<ApplicationUser?> GetByUserNameAsync(string username);
        Task<ApplicationUser?> GetByEmailAsync(string email);
        Task<IdentityResult> RegisterAsync(ApplicationUser user, string password);
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
        Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
        Task<bool> ResetPasswordAsync(ApplicationUser user, string token, string newPassword);
        Task<bool> UpdateUserInfoAsync(ApplicationUser user);
        Task<bool> AssignRoleAsync(ApplicationUser user, string role);
        Task<List<ApplicationUser>> GetAllUsersAsync();
        Task<IdentityResult> UpdateUserRoleAsync(ApplicationUser user, string newRole);
        Task<List<string>> GetAllRolesAsync();

    }
}