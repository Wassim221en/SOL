using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Template.Persistence.DbContext;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TemplateDbContext>
{
    public TemplateDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TemplateDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=SOLDb;Username=postgres;Password=0000");
        return new TemplateDbContext(optionsBuilder.Options);
    }
}