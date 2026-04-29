using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ZUMA.SharedKernel.Application.Configuration;

public static class DIContainer
{
    public static void ConfigureApplicationBaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging();
        services.AddSingleton<IMessageService, MessageService>();
    }
}
