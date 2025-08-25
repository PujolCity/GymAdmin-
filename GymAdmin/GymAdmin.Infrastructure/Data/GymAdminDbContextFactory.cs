using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GymAdmin.Infrastructure.Data;

public class GymAdminDbContextFactory : IDesignTimeDbContextFactory<GymAdminDbContext>
{
    public GymAdminDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GymAdminDbContext>();
        optionsBuilder.UseSqlite("Data Source=gymadmin.db", sqlite =>
        {
            sqlite.MigrationsAssembly(typeof(GymAdminDbContext).Assembly.FullName);
        });

        return new GymAdminDbContext(optionsBuilder.Options);
    }
}