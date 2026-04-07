using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ZUMA.Communication.Infrastructure.Persistence;

public abstract class BaseDesignTimeDbContextFactory<T> : IDesignTimeDbContextFactory<T>
    where T : DbContext
{
    protected abstract string ConnectionStringName { get; }

    public T CreateDbContext(string[] args)
    {
        string basePath = Directory.GetCurrentDirectory();

        // 2. Kontrola, zda appsettings.json existuje. Pokud ne, hledáme v Service projektu.
        if (!File.Exists(Path.Combine(basePath, "appsettings.json")))
        {
            var projectDirectory = new DirectoryInfo(basePath);

            // Trik: Pokud jsme v "ZUMA.Communication.Infrastructure", 
            // zkusíme najít "ZUMA.CommunicationService" v nadřazené složce.
            var serviceDirName = projectDirectory.Name.Replace(".Infrastructure", "Service");
            var potentialServicePath = Path.Combine(projectDirectory.Parent!.FullName, serviceDirName);

            if (File.Exists(Path.Combine(potentialServicePath, "appsettings.json")))
            {
                basePath = potentialServicePath;
            }
        }

        Console.WriteLine($"[EF Tools] loading configuration from: {basePath}");

        // 3. Sestavení konfigurace
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString(ConnectionStringName);

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                $"ERROR: ConnectionString '{ConnectionStringName}' not found at {Path.Combine(basePath, "appsettings.json")}");
        }

        // 4. Fix pro Docker vs Localhost (pokud v appsettings máš název kontejneru)
        var localConnectionString = connectionString.Contains("zuma-db")
            ? connectionString.Replace("zuma-db", "localhost")
            : connectionString;

        // 5. Konfigurace DbContextu
        var optionsBuilder = new DbContextOptionsBuilder<T>();
        optionsBuilder.UseNpgsql(localConnectionString);

        // 6. Vytvoření instance (předpokládá se konstruktor s DbContextOptions)
        return (T)Activator.CreateInstance(typeof(T), optionsBuilder.Options)!;
    }
}