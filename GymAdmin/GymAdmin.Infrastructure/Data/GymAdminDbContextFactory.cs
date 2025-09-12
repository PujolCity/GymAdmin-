using GymAdmin.Domain.Interfaces.Services;
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

        var dummyCrypto = new DummyCryptoService();
        return new GymAdminDbContext(optionsBuilder.Options, dummyCrypto);
    }
}


public class DummyCryptoService : ICryptoService
{
    public string Encrypt(string plainText) => plainText;
    public string Decrypt(string cipherText) => cipherText;
    public string ComputeHash(string plainText) => plainText;
}