using Focuswave.AuthService.Infrastructure.Vault;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Focuswave.AuthService.Persistence;

internal class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // путь до проекта
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.Development.json", optional: true) // <- добавьте это
            .Build();

        var vaultOptions = config.GetSection("Vault").Get<VaultOptions>();

        var vaultService =
            new VaultService(
                vaultOptions ?? throw new ApplicationException("Vault configuration is null")
            ) ?? throw new ApplicationException("Cannot create vault service");

        var connectionString = vaultService
            .GetSecretAsync("dev/auth-service", "AuthConnection")
            .Run()
            .AsTask()
            .Result.IfFail(err => throw new ApplicationException($"Ошибка: {err}"))
            .IfNone(() => throw new ApplicationException("Cant get connection string"));

        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new AuthDbContext(optionsBuilder.Options);
    }
}
