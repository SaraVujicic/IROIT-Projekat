using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RequestService.Domain.Interfaces;
using RequestService.Infrastructure.Data;
using RequestService.Infrastructure.Repositories;

namespace RequestService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("DefaultConnection string is not set.");
        }

        services.AddDbContext<RequestDbContext>(options =>
            options.UseNpgsql(connectionString,
                b => b.MigrationsAssembly(typeof(RequestDbContext).Assembly.FullName)));

        services.AddScoped<IRequestRepository, RequestRepository>();

        return services;
    }
}
