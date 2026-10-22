using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Roomfolio.Application.Abstractions;
using Roomfolio.Infrastructure.Common;
using Roomfolio.Infrastructure.Persistence;

namespace Roomfolio.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<RoomfolioDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IRoomfolioDbContext>(sp => sp.GetRequiredService<RoomfolioDbContext>());

        return services;
    }
}
