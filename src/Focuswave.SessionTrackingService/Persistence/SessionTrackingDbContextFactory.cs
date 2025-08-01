using Focuswave.Common.Infrastructure.Vault;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Focuswave.SessionTrackingService.Persistence;

internal class SessionTrackingDbContextFactory
    : IDesignTimeDbContextFactory<SessionTrackingDbContext>
{
    public SessionTrackingDbContext CreateDbContext(string[] args)
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
            .GetSecretAsync("dev/session-tracking-service", "SessionTrackingConnection")
            .Run()
            .AsTask()
            .Result.IfFail(err => throw new ApplicationException($"Ошибка: {err}"))
            .IfNone(() => throw new ApplicationException("Cant get connection string"));

        Console.WriteLine($"Connection string: {connectionString}");

        var optionsBuilder = new DbContextOptionsBuilder<SessionTrackingDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new SessionTrackingDbContext(optionsBuilder.Options);
    }
}
