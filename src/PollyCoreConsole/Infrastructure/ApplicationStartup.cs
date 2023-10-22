using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.Retry;
using Sagara.Core.Logging.Serilog;
using Serilog;

namespace PollyCoreConsole.Infrastructure;

public static class ApplicationStartup
{
    private static readonly ILogger _logger = Log.Logger.ForContext(typeof(ApplicationStartup));

    public static void ConfigureServices(HostApplicationBuilder builder)
    {
        builder.UseSerilog(HostBuilderHelper.ConfigureSerilog);


        //
        // Register HttpClients
        //

        //builder.Services.AddHttpClient("Foo")
        //    .AddTransientHttpErrorPolicy(PollyHelper.BuildWaitAndRetryPolicy);

        builder.Services.AddHttpClient(HttpClients.HttpStatus)
            .AddResilienceHandler(pipelineName: $"{HttpClients.HttpStatus} Pipeline", static pipelineBuilder =>
            {
                pipelineBuilder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>()
                {
                    ShouldHandle = args => ValueTask.FromResult(HttpClientResiliencePredicates.IsTransientHttpOutcome(args.Outcome)),
                    MaxRetryAttempts = 3,
                    BackoffType = DelayBackoffType.Exponential,
                    OnRetry = (args) =>
                    {
                        _logger.Warning(args.Outcome.Exception, "Failed to send request with HttpClient '{HttpClientName}'. Retrying after {RetryDelay}...", HttpClients.HttpStatus, args.RetryDelay);
                        return default;
                    }
                });
            });
    }
}
