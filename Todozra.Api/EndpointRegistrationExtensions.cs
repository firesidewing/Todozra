using System.Reflection;

namespace Todozra.Api;

public static class EndpointRegistrationExtensions
{
    public static IEndpointRouteBuilder MapAllEndpointsFromAssembly(
        this IEndpointRouteBuilder builder,
        Assembly assembly)
    {
        foreach (var type in assembly.DefinedTypes.Where(type => !type.IsAbstract && typeof(IEndPoint).IsAssignableFrom(type)))
        {
            if (Activator.CreateInstance(type.AsType(), nonPublic: true) is IEndPoint endpoint)
            {
                endpoint.MapEndPoint(builder);
            }
        }

        return builder;
    }
}