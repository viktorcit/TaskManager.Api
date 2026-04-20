using Microsoft.AspNetCore.Identity;
using TaskManager.Api.Model;

namespace TaskManager.Api.Data
{
    public static class Seed
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "User", "Employer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminLoginName = "AdminAccount";
            var adminPassword = "!AdminPassword123!";

            var existingUser = await userManager.FindByNameAsync(adminLoginName);

            if (existingUser == null)
            {
                var admin = new ApplicationUser
                {
                    Nickname = adminLoginName,
                    UserName = adminLoginName,
                    Name = "Admin",
                    CreatedAt = DateTimeOffset.UtcNow
                };

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Error creating admin user: {error.Description}");
                    }
                }

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }
    }
}
