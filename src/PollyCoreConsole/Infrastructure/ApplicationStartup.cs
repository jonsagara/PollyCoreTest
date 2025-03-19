using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                pipelineName: "httpstat.us Pipeline",
                pipelineBuilder => PollyHelper.ConfigureCircuitBreakerStrategy(
                    pipelineBuilder,
                    httpClientName: NamedHttpClients.HttpStatusCircuitBreaker.Name,
                    failureRatio: NamedHttpClients.HttpStatusCircuitBreaker.FailureRatio,
                    minimumThroughput: NamedHttpClients.HttpStatusCircuitBreaker.MinimumThroughput,
                    samplingDuration: NamedHttpClients.HttpStatusCircuitBreaker.SamplingDuration,
                    breakDuration: NamedHttpClients.HttpStatusCircuitBreaker.BreakDuration
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
