using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MoneyFlow.Infrastructure.Persistence;

public sealed class MoneyFlowDbContextFactory : IDesignTimeDbContextFactory<MoneyFlowDbContext>
{
    public MoneyFlowDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default was not found.");

        var optionsBuilder = new DbContextOptionsBuilder<MoneyFlowDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new MoneyFlowDbContext(optionsBuilder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var apiProjectPath = Path.GetFullPath(
            Path.Combine(Directory.GetCurrentDirectory(), "..", "MoneyFlow.Api"));

        return new ConfigurationBuilder()
            .SetBasePath(Directory.Exists(apiProjectPath)
                ? apiProjectPath
                : Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }
}
