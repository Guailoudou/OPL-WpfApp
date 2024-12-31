using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace OplWpf;

[AttributeUsage(AttributeTargets.Class)]
public class InjectionAttribute(ServiceLifetime serviceLifetime) : Attribute
{
    public ServiceLifetime ServiceLifetime => serviceLifetime;
}

public static class InjectionExtension
{
    public static IServiceCollection AddInjections(this IServiceCollection services)
    {
        var types = Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => type.GetCustomAttributes(typeof(InjectionAttribute), false).Length != 0);

        foreach (var type in types)
        {
            var attribute = type.GetCustomAttribute<InjectionAttribute>(false)!;
            var serviceLifetime = attribute.ServiceLifetime;
            switch (serviceLifetime)
            {
                case ServiceLifetime.Singleton:
                    services.AddSingleton(type);
                    break;
                case ServiceLifetime.Transient:
                    services.AddTransient(type);
                    break;
                case ServiceLifetime.Scoped:
                    services.AddScoped(type);
                    break;
            }
        }
        return services;
    }
}
