namespace PollyCoreConsole;

public static class NamedHttpClients
{
    private static readonly HttpClientPropertiesWithRetry _httpStatusProperties = new HttpClientPropertiesWithRetry(Name: "httpstat.us", MaxRetryAttempts: 3);

    /// <summary>
    /// HttpClient for making requests to httpstat.us's API. Retries up to 3 times.
    /// </summary>
    public static HttpClientPropertiesWithRetry HttpStatus
        => _httpStatusProperties;


    //
    // Classes
    //

    public readonly record struct HttpClientPropertiesWithRetry(string Name, int MaxRetryAttempts);
}
