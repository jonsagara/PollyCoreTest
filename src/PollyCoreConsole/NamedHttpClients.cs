namespace PollyCoreConsole;

public static class NamedHttpClients
{
    private static readonly HttpClientPropertiesWithRetry _httpStatusRetryProperties = new HttpClientPropertiesWithRetry(Name: "httpstat.us-retry", MaxRetryAttempts: 3);

    /// <summary>
    /// HttpClient for making requests to httpstat.us's API. Retries up to 3 times.
    /// </summary>
    public static HttpClientPropertiesWithRetry HttpStatusRetry
        => _httpStatusRetryProperties;


    private static readonly HttpClientProperties _httpStatusCircuitBreakerProperties = new HttpClientProperties(Name: "httpstat.us-circuit-breaker");

    /// <summary>
    /// HttpClient for making requests to httpstat.us's API. Retries up to 3 times.
    /// </summary>
    public static HttpClientProperties HttpStatusCircuitBreaker
        => _httpStatusCircuitBreakerProperties;


    //
    // Classes
    //

    public readonly record struct HttpClientProperties(string Name);

    public readonly record struct HttpClientPropertiesWithRetry(string Name, int MaxRetryAttempts);
}
