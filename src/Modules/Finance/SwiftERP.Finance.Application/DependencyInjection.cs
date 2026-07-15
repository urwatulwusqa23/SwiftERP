using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace SwiftERP.Finance.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddFinanceApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
