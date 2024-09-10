﻿using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using AuditTrail;
using AuditTrail.Model;

namespace Microsoft.Extensions.DependencyInjection;

public static class AuditTrailExtensions
{
    public static IServiceCollection AddAuditTrail<T>(this IServiceCollection services) where T : class, IAuditTrailLog
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        return AddAuditTrail<T>(services, setupAction: null);
    }

    public static IServiceCollection AddAuditTrail<T>(
        this IServiceCollection services,
        Action<AuditTrailOptions> setupAction) where T : class, IAuditTrailLog
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

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
