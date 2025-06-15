using DevHabit.Api.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Database;

public sealed class ApplicationIdentityDbContext(
    DbContextOptions<ApplicationIdentityDbContext> options) : IdentityDbContext(options)
{
    public DbSet<RefreshToken> RefreshTokens { get; set; } 

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema(Schemas.Identity);

        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId)
                .HasMaxLength(300);
            
            entity.Property(e => e.Token)
                .HasMaxLength(1000);
            
            entity.HasIndex(e => e.Token)
                .IsUnique();
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
