using Microsoft.Extensions.DependencyInjection;
using OpenHabitTracker.App;
using OpenHabitTracker.Blazor.Web.ApiClient;

namespace OpenHabitTracker.Blazor;

public static class Startup
{
    public static IServiceCollection AddBlazor(this IServiceCollection services)
    {
        services.AddScoped<JsInterop>();
        services.AddScoped<IRuntimeClientData, RuntimeClientData>();

        return services;
    }
}
