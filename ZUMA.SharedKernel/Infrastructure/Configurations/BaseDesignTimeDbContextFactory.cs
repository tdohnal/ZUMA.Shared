using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

public abstract class BaseDesignTimeDbContextFactory<T> : IDesignTimeDbContextFactory<T>
    where T : DbContext
{
    protected abstract string ConnectionStringName { get; }

    public T CreateDbContext(string[] args)
    {
        // 1. Najdeme Root (ZUMA.API), kde leží .sln
        var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
        var rootDir = currentDir;
        while (rootDir != null && !rootDir.GetFiles("*.sln").Any())
        {
            rootDir = rootDir.Parent;
        }

        if (rootDir == null)
            throw new Exception("Chyba: Soubor .sln nebyl nalezen. Jsi v repozitáři ZUMA.API?");

        // 2. Teď najdeme ten správný Service projekt. 
        // Trik: Hledáme složku, která začíná stejně jako náš projekt, ale končí na "Service"
        // Např. z "ZUMA.Customer.Infrastructure" uděláme "ZUMA.CustomerService"
        string currentProjectName = typeof(T).Name.Replace("DbContext", "Service");
        // Pokud se tvůj projekt jmenuje jinak, klidně to jméno sem napiš natvrdo:
        // string targetProject = "ZUMA.CustomerService"; 

        var allSettingsFiles = rootDir.GetFiles("appsettings.json", SearchOption.AllDirectories);

        // Najdeme ten, který je v té správné složce (např. .../Services/Customer/ZUMA.CustomerService/)
        var settingsFile = allSettingsFiles.FirstOrDefault(f =>
            f.DirectoryName!.Contains("Services") &&
            f.DirectoryName.EndsWith(currentProjectName)
        );

        if (settingsFile == null)
        {
            // Failback: Pokud to nenajde podle jména, vezmi první appsettings, co vypadá jako Service
            settingsFile = allSettingsFiles.FirstOrDefault(f => f.DirectoryName!.EndsWith("Service"));
        }

        if (settingsFile == null)
            throw new Exception($"[EF Tools] ERROR: appsettings.json nebyl nalezen pro projekt {currentProjectName}");

        string basePath = settingsFile.DirectoryName!;
        Console.WriteLine($"[EF Tools] SUCCESS: Konfigurace načtena z: {basePath}");

        // 3. Sestavení konfigurace a DbContextu (zbytek znáš)
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<T>();
        optionsBuilder.UseNpgsql(configuration.GetConnectionString(ConnectionStringName));

        return (T)Activator.CreateInstance(typeof(T), optionsBuilder.Options)!;
    }
}