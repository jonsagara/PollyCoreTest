using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
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

        //builder.Services.AddHttpClient("Foo")
        //    .AddTransientHttpErrorPolicy(PollyHelper.BuildWaitAndRetryPolicy);

        builder.Services.AddHttpClient(HttpClients.HttpStatus)
            .AddResilienceHandler(
                pipelineName: $"{HttpClients.HttpStatus} Pipeline",
                pipelineBuilder => PollyHelper.ConfigureRetryAndWaitWithExponentialBackoffStrategy(pipelineBuilder, maxRetryAttempts: 3, httpClientName: HttpClients.HttpStatus)
                );
    }
}
