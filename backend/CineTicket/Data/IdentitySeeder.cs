using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using CineTicket.Models;
namespace CineTicket.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = {
                CineTicket.Models.SD.Role_Customer,
                CineTicket.Models.SD.Role_Company,
                CineTicket.Models.SD.Role_Admin,
                CineTicket.Models.SD.Role_Employee
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
