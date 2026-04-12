using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ZUMA.SharedKernel.Configurration;

public static class DIContainer
{
    public static void ConfigureBaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging();
        services.AddSingleton<IMessageService, MessageService>();
    }
}
