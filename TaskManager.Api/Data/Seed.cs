using Microsoft.AspNetCore.Identity;
using TaskManager.Api.Entity;

namespace TaskManager.Api.Data
{
    public class Seed
    {
        private readonly IConfiguration _config;
        private readonly string _adminUserName;
        private readonly string _adminPassword;

        public Seed(IConfiguration config)
        {
            _config = config;
            _adminUserName = _config["AdminUsername"]
                ?? throw new InvalidOperationException("AdminUsername не задан в конфигурации (AdminUsername).");
            _adminPassword = _config["AdminPassword"]
                ?? throw new InvalidOperationException("AdminPassword не задан в конфигурации (AdminPassword).");
        }


        public async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "User", "Employer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminUserName = _adminUserName;
            var adminPassword = _adminPassword;

            var existingUser = await userManager.FindByNameAsync(adminUserName);

            if (existingUser == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminUserName,
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
