using SOL.Domain.Entities.Security;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Template.Application.Common.DbContext;
using Template.Domain.Primitives.Entity.Identity;

namespace Template.Persistence.DbContext;

public class TemplateDbContext : IdentityDbContext<User, Role, Guid>, ITemplateDbContext
{
    public TemplateDbContext(DbContextOptions<TemplateDbContext> options) : base(options)
    {
        
    }
    public DbSet<AppUser> AppUsers { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        

        builder.Entity<Role>(b =>
        {
            b.HasMany(r => r.UserRoles)
                .WithOne()
                .HasForeignKey(ur => ur.RoleId);
            b.HasMany(r => r.RoleClaims)
                .WithOne()
                .HasForeignKey(rc => rc.RoleId);
        });
        builder.Entity<User>()
            .HasMany(u => u.UserRoles)
            .WithOne()
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();
        builder.Entity<User>()
            .HasMany(u => u.UserClaims)
            .WithOne()
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();
        builder.Entity<Role>()
            .HasMany(r=>r.RoleClaims)
            .WithOne()
            .HasForeignKey(ur=>ur.RoleId)
            .IsRequired();
        builder.Entity<User>(b =>
        {
           

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
