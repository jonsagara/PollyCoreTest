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

    public static void ConfigureCircuitBreakerStrategy(ResiliencePipelineBuilder<HttpResponseMessage> pipelineBuilder, string httpClientName, double failureRatio, int minimumThroughput, TimeSpan samplingDuration, TimeSpan breakDuration)
    {
        Check.NotNull(pipelineBuilder);
        Check.NotEmpty(httpClientName);
        Check.NotOutOfRange(failureRatio, rangeLo: 0.0, rangeHi: 1.0);
        Check.NotOutOfRange(minimumThroughput, rangeLo: 1, rangeHi: int.MaxValue);

        pipelineBuilder.AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
        {
            // Specifies which results and exceptions are managed by the circuit breaker strategy. In this case, transient HTTP errors.
            ShouldHandle = args => ValueTask.FromResult(HttpClientResiliencePredicates.IsTransientHttpOutcome(args.Outcome)),

            // The ratio of failures to successes that will cause the circuit to break/open.
            // Ex: 0.1 means if > 10% of requests fail within the sampling duration window, assuming the minimum throughput is satisfied,
            //   the circuit will transition to the Opened state.
            FailureRatio = failureRatio,

            // The minimum number of actions that must occur in the circuit within a specific time slice.
            MinimumThroughput = minimumThroughput,

            // The time period over which failure ratios are calculated.
            SamplingDuration = samplingDuration,

            // The time period for which the circuit will remain broken/open before attempting to reset.
            BreakDuration = breakDuration,


            //
            // Log transitions to the three different states
            //

            // Event triggered when the circuit transitions to the Opened state.
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

            // Event triggered when the circuit transitions to the HalfOpened state. The next request is a test: if it succeeds,
            //   transition to Closed; otherwise, transition back to Opened.
            OnHalfOpened = args =>
            {
                _logger.Warning("[HttpClient={HttpClientName}] Circuit transitioned to Half-Opened.", httpClientName);

                return ValueTask.CompletedTask;
            },

            // Event triggered when the circuit transitions to the Closed state.
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
