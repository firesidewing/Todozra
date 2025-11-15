using System.Reflection;

using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Todozra.Api;

public static class EndpointRegistrationExtensions
{
    public static IServiceCollection AddEndpoints(
        this IServiceCollection services,
        Assembly assembly)
    {
        var endpointTypes = assembly
            .DefinedTypes
            .Where(t =>
                t is { IsAbstract: false, IsInterface: false } &&
                typeof(IEndPoint).IsAssignableFrom(t));

        var descriptors = endpointTypes
            .Select(t => ServiceDescriptor.Transient(typeof(IEndPoint), t.AsType()))
            .ToArray();

        services.TryAddEnumerable(descriptors);

        return services;
    }

    public static IApplicationBuilder MapEndpoints(
        this WebApplication app,
        RouteGroupBuilder? routeGroupBuilder = null)
    {
        var endpoints = app.Services.GetRequiredService<IEnumerable<IEndPoint>>();
        IEndpointRouteBuilder builder = routeGroupBuilder is null
            ? app
            : routeGroupBuilder;

        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndPoint(builder);
        }

        return app;
    }

}