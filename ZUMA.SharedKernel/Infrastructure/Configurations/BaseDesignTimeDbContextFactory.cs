using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

public abstract class BaseDesignTimeDbContextFactory<T> : IDesignTimeDbContextFactory<T>
    where T : DbContext
{
    protected abstract string ConnectionStringName { get; }

    public T CreateDbContext(string[] args)
    {
        // Žádné hledání souborů, žádné rootDir. Jen čistý Environment.
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var connString = configuration.GetConnectionString(ConnectionStringName);

        // Pokud to chceš mít "neprůstřelné" pro generování migrací za běhu,
        // musíš tu mít aspoň fallback, jinak migrations add neuděláš bez nahozených ENVs.
        if (string.IsNullOrEmpty(connString))
        {
            Console.WriteLine("ConnectionString IS NULL! Also will be used 'Host=localhost;Database=dummy'");
            // Tento string se použije JEN když v prostředí NIC není (třeba při add-migration v IDE)
            connString = "Host=localhost;Database=dummy";
        }

        var optionsBuilder = new DbContextOptionsBuilder<T>();
        optionsBuilder.UseNpgsql(connString);

        return (T)Activator.CreateInstance(typeof(T), optionsBuilder.Options)!;
    }
}