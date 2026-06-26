using SOL.Domain.Entities.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Template.Domain.Enums;
using Template.Domain.Primitives.Entity.Identity;

namespace Template.API;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

        if (!await roleManager.RoleExistsAsync("SUPERADMIN"))
        {
            await roleManager.CreateAsync(new Role("SUPERADMIN", "Super Administrator", RoleStatus.Active));
        }

        if (!await roleManager.RoleExistsAsync("ADMIN"))
        {
            await roleManager.CreateAsync(new Role("ADMIN", "Administrator", RoleStatus.Active));
        }

        if (!await userManager.Users.AnyAsync(u => u.UserName == "superadmin"))
        {
            var admin = new AppUser();
            admin.UpdateBasicInformation(
                firstName: "Super",
                lastName: "Admin",
                email: "admin@template.com",
                userName: "superadmin",
                phoneNumber: "0000000000");
            admin.SetStatus(ActiveStatus.Active);

            var result = await userManager.CreateAsync(admin, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "SUPERADMIN");
            }
        }
    }
}