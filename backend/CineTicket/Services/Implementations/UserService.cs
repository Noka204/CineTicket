using CineTicket.Models;
using CineTicket.Repositories.Interfaces;
using CineTicket.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserService(IUserRepository repo, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _repo = repo;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public Task<ApplicationUser?> GetByIdAsync(string id) => _repo.GetByIdAsync(id);

        public Task<ApplicationUser?> GetByUserNameAsync(string username) => _repo.GetByUserNameAsync(username);

        public Task<ApplicationUser?> GetByEmailAsync(string email) => _repo.GetByEmailAsync(email);

        public Task<IdentityResult> RegisterAsync(ApplicationUser user, string password) => _repo.RegisterAsync(user, password);

        public Task<bool> CheckPasswordAsync(ApplicationUser user, string password) => _repo.CheckPasswordAsync(user, password);

        public Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user) => _repo.GeneratePasswordResetTokenAsync(user);

        public Task<bool> ResetPasswordAsync(ApplicationUser user, string token, string newPassword) => _repo.ResetPasswordAsync(user, token, newPassword);

        public Task<bool> UpdateUserInfoAsync(ApplicationUser user) => _repo.UpdateUserInfoAsync(user);

        public async Task<bool> AssignRoleAsync(ApplicationUser user, string role)
        {
            var result = await _userManager.AddToRoleAsync(user, role);
            return result.Succeeded;
        }
        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            return await _userManager.Users.ToListAsync();
        }

        public async Task<IdentityResult> UpdateUserRoleAsync(ApplicationUser user, string newRole)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded) return removeResult;

            var addResult = await _userManager.AddToRoleAsync(user, newRole);
            if (addResult.Succeeded)
            {
                user.Role = newRole;
                await _userManager.UpdateAsync(user);
            }

            return addResult;
        }

        public async Task<List<string>> GetAllRolesAsync()
        {
            return await Task.FromResult(_roleManager.Roles.Select(r => r.Name).ToList());
        }


    }
}
