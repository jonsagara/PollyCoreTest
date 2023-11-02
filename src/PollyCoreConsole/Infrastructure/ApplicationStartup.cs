using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using PollyCoreConsole.Services;
using Sagara.Core.Logging.Serilog;
using Serilog;

namespace PollyCoreConsole.Infrastructure;

public static class ApplicationStartup
{
    public static void ConfigureServices(HostApplicationBuilder builder)
    {
        builder.UseSerilog(HostBuilderHelper.ConfigureSerilog);


        //
        // Register HttpClients
        //

        builder.Services.AddHttpClient(NamedHttpClients.HttpStatus.Name)
            .AddResilienceHandler(
                pipelineName: $"{NamedHttpClients.HttpStatus.Name} Pipeline",
                pipelineBuilder => PollyHelper.ConfigureRetryAndWaitWithExponentialBackoffStrategy(pipelineBuilder, maxRetryAttempts: NamedHttpClients.HttpStatus.MaxRetryAttempts, httpClientName: NamedHttpClients.HttpStatus.Name)
                );


        //
        // Scan for services defined in this project.
        //

        builder.Services.Scan(scan => scan
            .FromAssemblyOf<IPollyCoreConsoleService>()
            .AddClasses(classes => classes.AssignableTo<IPollyCoreConsoleService>())
            .AsSelf()
            .WithScopedLifetime());
    }
}
