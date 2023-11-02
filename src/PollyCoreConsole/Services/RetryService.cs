using Serilog;

namespace PollyCoreConsole.Services;

public class RetryService : IPollyCoreConsoleService
{
    private static readonly ILogger _logger = Log.Logger.ForContext<RetryService>();

    private readonly IHttpClientFactory _httpClientFactory;

    public RetryService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task RetryRequestDemoAsync(string requestUrl)
    {
        var httpClient = _httpClientFactory.CreateClient(NamedHttpClients.HttpStatus.Name);

        _logger.Information("Making HTTP request to {RequestUrl}...", requestUrl);
        using var requestMsg = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        using var responseMsg = await httpClient.SendAsync(requestMsg);

        _logger.Information("Status: {StatusCodeInt} {StatusCode}", (int)responseMsg.StatusCode, responseMsg.StatusCode);
        var responseBody = await responseMsg.Content.ReadAsStringAsync();
        _logger.Information("Response body: {ResponseBody}", responseBody);
    }
}
