using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using RequestService.Application.Behaviors;

namespace RequestService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddAutoMapper(assembly);
        
        services.AddValidatorsFromAssembly(assembly);
        
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // HttpClient registration moved to API project to configure Correlation ID propagation delegating handler

        return services;
    }
}
