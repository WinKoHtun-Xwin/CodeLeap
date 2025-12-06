using CodeLeap.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace CodeLeap.API
{
    public static class RoleInitializer
    {
        public static async Task InitializeRolesAsync(RoleManager<IdentityRole> roleManager, UserManager<UserEntity> userManager)
        {
            string[] roleNames = { "Admin", "Manager", "User" };

            // Create roles if they don't exist
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Optional: Create a default admin user if needed
            // This is useful for initial setup
            var adminEmail = "admin";
            var adminUser = await userManager.FindByNameAsync(adminEmail);
            
            if (adminUser == null)
            {
                var adminId = Guid.NewGuid().ToString();
                adminUser = new UserEntity
                {
                    Id = adminId,
                    UserName = adminEmail,
                    Email = "admin@codeleap.com",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBy = adminId,  // Admin creates itself
                    CreatedAt = DateTime.UtcNow
                };
                
                // Create admin user with a default password (should be changed immediately)
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}
