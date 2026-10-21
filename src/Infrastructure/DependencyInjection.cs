using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Roomfolio.Application.Abstractions;
using Roomfolio.Infrastructure.Common;

namespace Roomfolio.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        return services;
    }
}
