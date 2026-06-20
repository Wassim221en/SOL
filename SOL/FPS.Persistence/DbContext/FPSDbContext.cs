using FPS.Domain.Entities.Security;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Template.Application.Common.DbContext;
using Template.Domain.Primitives.Entity.Identity;

namespace Template.Persistence.DbContext;

public class TemplateDbContext : IdentityDbContext<User, BaseRole, Guid>, ITemplateDbContext
{
    public TemplateDbContext(DbContextOptions<TemplateDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>(e => e.HasBaseType<User>());
        builder.Entity<Role>(e => e.HasBaseType<BaseRole>());

        builder.Entity<BaseRole>(b =>
        {
            b.HasMany(r => r.UserRoles)
                .WithOne()
                .HasForeignKey(ur => ur.RoleId);
            b.HasMany(r => r.RoleClaims)
                .WithOne()
                .HasForeignKey(rc => rc.RoleId);
        });

        builder.Entity<User>(b =>
        {
            b.HasMany(u => u.UserRoles)
                .WithOne()
                .HasForeignKey(ur => ur.UserId);
            b.HasMany(u => u.UserClaims)
                .WithOne()
                .HasForeignKey(uc => uc.UserId);

            b.Property<long>("Number")
                .HasColumnType("bigint")
                .IsRequired()
                .ValueGeneratedOnAdd()
                .HasAnnotation(
                    "Npgsql:ValueGenerationStrategy",
                    Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        });

        builder.Entity<User>()
            .Property(u => u.ImageUrl)
            .HasConversion(
                value => System.Text.Json.JsonSerializer.Serialize(value, (System.Text.Json.JsonSerializerOptions?)null),
                value => string.IsNullOrWhiteSpace(value)
                    ? new User.ImageUrls()
                    : System.Text.Json.JsonSerializer.Deserialize<User.ImageUrls>(value, (System.Text.Json.JsonSerializerOptions?)null) ?? new User.ImageUrls());

        builder.Entity<User>()
            .Property(u => u.DeviceTokens)
            .HasConversion(
                value => System.Text.Json.JsonSerializer.Serialize(value, (System.Text.Json.JsonSerializerOptions?)null),
                value => string.IsNullOrWhiteSpace(value)
                    ? new List<string?>()
                    : System.Text.Json.JsonSerializer.Deserialize<List<string?>>(value, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string?>());
    }
}
