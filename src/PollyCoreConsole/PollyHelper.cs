using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.CircuitBreaker;
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

    public static void ConfigureCircuitBreakerStrategy(ResiliencePipelineBuilder<HttpResponseMessage> pipelineBuilder, string? httpClientName)
    {
        Check.NotNull(pipelineBuilder);

        pipelineBuilder.AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
        {
            ShouldHandle = args => ValueTask.FromResult(HttpClientResiliencePredicates.IsTransientHttpOutcome(args.Outcome)),
            
            OnOpened = args =>
            {
                _logger.Warning(args.Outcome.Exception, "[HttpClient={HttpClientName}] Circuit transitioned to Opened after failing to send request to {RequestUri}. StatusCode: {StatusCodeInt} {StatusCode}. Break duration: {BreakDuration}. Is manual: {IsManual}.",
                    httpClientName,
                    args.Outcome.Result?.RequestMessage?.RequestUri,
                    (int?)args.Outcome.Result?.StatusCode,
                    args.Outcome.Result?.StatusCode,
                    args.BreakDuration,
                    args.IsManual
                    );

                return ValueTask.CompletedTask;
            },
            OnHalfOpened = args =>
            {
                _logger.Warning("[HttpClient={HttpClientName}] Circuit transitioned to Half-Opened.", httpClientName);

                return ValueTask.CompletedTask;
            },
            OnClosed = args =>
            {
                _logger.Warning("[HttpClient={HttpClientName}] Circuit transitioned to Closed after sending request to {RequestUri}. StatusCode: {StatusCodeInt} {StatusCode}. Is manual: {IsManual}.",
                    httpClientName,
                    args.Outcome.Result?.RequestMessage?.RequestUri,
                    (int?)args.Outcome.Result?.StatusCode,
                    args.Outcome.Result?.StatusCode,
                    args.IsManual
                    );

                return ValueTask.CompletedTask;
            },
        });
    }
}
