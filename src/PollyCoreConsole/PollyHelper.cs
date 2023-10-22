using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.Retry;
using Sagara.Core;
using Serilog;

namespace PollyCoreConsole;

public static class PollyHelper
{
    private static readonly ILogger _logger = Log.Logger.ForContext(typeof(PollyHelper));

    public static void ConfigureRetryAndWaitWithExponentialBackoffStrategy(ResiliencePipelineBuilder<HttpResponseMessage> pipelineBuilder, int maxRetryAttempts, string? httpClientName)
    {
        Check.NotNull(pipelineBuilder);
        Check.NotOutOfRange(maxRetryAttempts, rangeLo: 1, rangeHi: int.MaxValue);

        pipelineBuilder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>()
        {
            ShouldHandle = args => ValueTask.FromResult(HttpClientResiliencePredicates.IsTransientHttpOutcome(args.Outcome)),
            MaxRetryAttempts = maxRetryAttempts,
            BackoffType = DelayBackoffType.Exponential,
            OnRetry = args =>
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
