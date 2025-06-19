using CineTicket.Models;
using CineTicket.Repositories.Interfaces;
using CineTicket.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CineTicket.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }

        public Task<ApplicationUser?> GetByIdAsync(string id) => _repo.GetByIdAsync(id);

        public Task<ApplicationUser?> GetByUserNameAsync(string username) => _repo.GetByUserNameAsync(username);

        public Task<ApplicationUser?> GetByEmailAsync(string email) => _repo.GetByEmailAsync(email);

        public Task<IdentityResult> RegisterAsync(ApplicationUser user, string password) => _repo.RegisterAsync(user, password);

        public Task<bool> CheckPasswordAsync(ApplicationUser user, string password) => _repo.CheckPasswordAsync(user, password);

        public Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user) => _repo.GeneratePasswordResetTokenAsync(user);

        public Task<bool> ResetPasswordAsync(ApplicationUser user, string token, string newPassword) => _repo.ResetPasswordAsync(user, token, newPassword);

        public Task<bool> UpdateUserInfoAsync(ApplicationUser user) => _repo.UpdateUserInfoAsync(user);
    }
}
