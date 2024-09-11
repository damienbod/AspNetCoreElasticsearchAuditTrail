using AuditTrail;
using AuditTrail.Model;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Microsoft.Extensions.DependencyInjection;

public static class AuditTrailExtensions
{
    public static IServiceCollection AddAuditTrail<T>(this IServiceCollection services) where T : class, IAuditTrailLog
    {
        ArgumentNullException.ThrowIfNull(services);

        return AddAuditTrail<T>(services, setupAction: null);
    }

    public static IServiceCollection AddAuditTrail<T>(
        this IServiceCollection services,
        Action<AuditTrailOptions> setupAction) where T : class, IAuditTrailLog
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAdd(new ServiceDescriptor(
            typeof(IAuditTrailProvider<T>),
            typeof(AuditTrailProvider<T>),
            ServiceLifetime.Transient));

        if (setupAction != null)
        {
            services.Configure(setupAction);
        }

        return services;
    }
}
