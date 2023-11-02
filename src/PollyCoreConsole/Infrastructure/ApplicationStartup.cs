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

        builder.Services.AddHttpClient(NamedHttpClients.HttpStatusRetry.Name)
            .AddResilienceHandler(
                pipelineName: $"{NamedHttpClients.HttpStatusRetry.Name} Pipeline",
                pipelineBuilder => PollyHelper.ConfigureRetryAndWaitWithExponentialBackoffStrategy(pipelineBuilder, maxRetryAttempts: NamedHttpClients.HttpStatusRetry.MaxRetryAttempts, httpClientName: NamedHttpClients.HttpStatusRetry.Name)
                );

        builder.Services.AddHttpClient(NamedHttpClients.HttpStatusCircuitBreaker.Name)
            .AddResilienceHandler(
                pipelineName: $"Pipeline",
                pipelineBuilder => PollyHelper.ConfigureCircuitBreakerStrategy(
                    pipelineBuilder,
                    failureRatio: 0.2,
                    minimumThroughput: 10,
                    samplingDuration: TimeSpan.FromSeconds(60.0),
                    breakDuration: TimeSpan.FromSeconds(5.0),
                    httpClientName: NamedHttpClients.HttpStatusCircuitBreaker.Name
                    )
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
