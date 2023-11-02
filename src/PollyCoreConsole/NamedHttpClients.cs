namespace PollyCoreConsole;

public static class NamedHttpClients
{
    private static readonly HttpClientPropertiesWithRetry _httpStatusRetryProperties = new HttpClientPropertiesWithRetry(Name: "httpstat.us-retry", MaxRetryAttempts: 3);

    /// <summary>
    /// HttpClient for making requests to httpstat.us's API. Retries up to 3 times.
    /// </summary>
    public static HttpClientPropertiesWithRetry HttpStatusRetry
        => _httpStatusRetryProperties;


    private static readonly HttpClientPropertiesWithCircuitBreaker _httpStatusCircuitBreakerProperties = new HttpClientPropertiesWithCircuitBreaker(
        Name: "httpstat.us-circuit-breaker",
        FailureRatio: 0.2,
        MinimumThroughput: 10,
        SamplingDuration: TimeSpan.FromSeconds(60.0),
        BreakDuration: TimeSpan.FromSeconds(30.0)
        );

    /// <summary>
    /// HttpClient for making requests to httpstat.us's API. If 3 or more requests fail within a 60 second window, break the circuit
    /// for 30 seconds before retrying.
    /// </summary>
    public static HttpClientPropertiesWithCircuitBreaker HttpStatusCircuitBreaker
        => _httpStatusCircuitBreakerProperties;


    //
    // Classes
    //

    public readonly record struct HttpClientProperties(string Name);

    public readonly record struct HttpClientPropertiesWithRetry(string Name, int MaxRetryAttempts);

    /// <summary>
    /// Configuration for a Polly Circuit Breaker.
    /// </summary>
    /// <param name="Name">The HttpClient name.</param>
    /// <param name="FailureRatio">The ratio of failures to successes that will cause the circuit to break/open. Must be in [0, 1].</param>
    /// <param name="MinimumThroughput">The minimum number of actions that must occur in the circuit within a specific time slice. Must be in [1, int.MaxValue].</param>
    /// <param name="SamplingDuration">The time period over which failure ratios are calculated.</param>
    /// <param name="BreakDuration">The time period for which the circuit will remain broken/open before attempting to reset.</param>
    public readonly record struct HttpClientPropertiesWithCircuitBreaker(
        string Name,
        double FailureRatio,
        int MinimumThroughput,
        TimeSpan SamplingDuration,
        TimeSpan BreakDuration
        );
}
