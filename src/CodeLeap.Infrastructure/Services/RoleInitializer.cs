using CodeLeap.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace CodeLeap.Infrastructure.Services
{
    public static class RoleInitializer
    {
        public static async Task InitializeRolesAsync(RoleManager<IdentityRole> roleManager, UserManager<UserEntity> userManager)
        {
            string[] roleNames = { "Admin", "User" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}
