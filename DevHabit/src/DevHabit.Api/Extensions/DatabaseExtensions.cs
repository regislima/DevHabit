using DevHabit.Api.Database;
using DevHabit.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Extensions;

public static class DatabaseExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        await using var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await using var identityDbContext = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();

        try
        {
            await applicationDbContext.Database.MigrateAsync();
            app.Logger.LogInformation("Application Database migrations applied successfully.");

            await identityDbContext.Database.MigrateAsync();
            app.Logger.LogInformation("Identity Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "An error occurred while applying database migrations");
            throw;
        }
    }

    public static async Task SeedinitialDataAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        
        try
        {
            if (!await roleManager.RoleExistsAsync(Roles.Member))
                await roleManager.CreateAsync(new IdentityRole(Roles.Member));

            if (!await roleManager.RoleExistsAsync(Roles.Admin))
                await roleManager.CreateAsync(new IdentityRole(Roles.Admin));

            app.Logger.LogInformation("Initial roles seeded successfully.");
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "An error occurred while seeding initial data");
            throw;
        }
    }
}
