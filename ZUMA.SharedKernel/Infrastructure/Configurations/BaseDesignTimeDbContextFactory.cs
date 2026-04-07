using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

public abstract class BaseDesignTimeDbContextFactory<T> : IDesignTimeDbContextFactory<T>
    where T : DbContext
{
    protected abstract string ConnectionStringName { get; }

    public T CreateDbContext(string[] args)
    {
        string basePath = Directory.GetCurrentDirectory();

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString(ConnectionStringName);

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                $"Error: ConnectionString '{ConnectionStringName}' not found in {Path.Combine(basePath, "appsettings.json")}");
        }

        var localConnectionString = connectionString.Contains("zuma-db")
            ? connectionString.Replace("zuma-db", "localhost")
            : connectionString;

        var optionsBuilder = new DbContextOptionsBuilder<T>();
        optionsBuilder.UseNpgsql(localConnectionString);

        return (T)Activator.CreateInstance(typeof(T), optionsBuilder.Options)!;
    }
}