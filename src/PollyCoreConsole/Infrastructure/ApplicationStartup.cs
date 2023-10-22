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
            .AddResilienceHandler(
                pipelineName: $"{HttpClients.HttpStatus} Pipeline", 
                pipelineBuilder => PollyHelper.ConfigureRetryAndWaitWithExponentialBackoffStrategy(pipelineBuilder, maxRetryAttempts: 3, httpClientName: HttpClients.HttpStatus)
                );
    }
}

public static class PollyHelper
{
    private static readonly ILogger _logger = Log.Logger.ForContext(typeof(PollyHelper));

    public static void ConfigureRetryAndWaitWithExponentialBackoffStrategy(ResiliencePipelineBuilder<HttpResponseMessage> pipelineBuilder, int maxRetryAttempts, string? httpClientName)
    {
        pipelineBuilder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>()
        {
            ShouldHandle = args => ValueTask.FromResult(HttpClientResiliencePredicates.IsTransientHttpOutcome(args.Outcome)),
            MaxRetryAttempts = maxRetryAttempts,
            BackoffType = DelayBackoffType.Exponential,
            OnRetry = (args) =>
            {
                _logger.Warning(args.Outcome.Exception, "[HttpClient={HttpClientName}] Failed to send request to {RequestUri}. StatusCode: {StatusCodeInt} {StatusCode}. The attempt took {Duration}. Retrying after {RetryDelay}...",
                    httpClientName,
                    args.Outcome.Result?.RequestMessage?.RequestUri,
                    (int?)args.Outcome.Result?.StatusCode,
                    args.Outcome.Result?.StatusCode,
                    args.Duration,
                    args.RetryDelay
                    );

                return ValueTask.CompletedTask;
            }
        });
    }
}
