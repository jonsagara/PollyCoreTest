using System.Net.Http;
using Polly.CircuitBreaker;
using Serilog;

namespace PollyCoreConsole.Services;

public class CircuitBreakerService : IPollyCoreConsoleService
{
    private static readonly ILogger _logger = Log.Logger.ForContext<CircuitBreakerService>();

    private readonly IHttpClientFactory _httpClientFactory;

    public CircuitBreakerService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task CircuitBreakerDemoAsync(string requestUrl)
    {
        const string successUrl = "https://localhost:44366/200";
        const string failureUrl = "https://localhost:44366/500";

        // This triggers on the 10th request, which is the second failed request in the sample of 10.
        //await MakeRequestAsync(0, successUrl);
        //await MakeRequestAsync(1, successUrl);
        //await MakeRequestAsync(2, successUrl);
        //await MakeRequestAsync(3, successUrl);
        //await MakeRequestAsync(4, successUrl);
        //await MakeRequestAsync(5, successUrl);
        //await MakeRequestAsync(6, successUrl);
        //await MakeRequestAsync(7, successUrl);
        //await MakeRequestAsync(8, failureUrl);
        //await MakeRequestAsync(9, failureUrl);

        // Do 10 successful requests to meet the minimum.
        var ixRequest = 0;
        for (; ixRequest < 10; ixRequest++)
        {
            await MakeRequestAsync(ixRequest, successUrl);
        }

        // Now see when the circuit breaks.
        await MakeRequestAsync(++ixRequest, failureUrl);
        await MakeRequestAsync(++ixRequest, failureUrl);
        //await MakeRequestAsync(++ixRequest, successUrl);
        await MakeRequestAsync(++ixRequest, failureUrl);
        //await MakeRequestAsync(++ixRequest, failureUrl);
        await MakeRequestAsync(++ixRequest, successUrl);
        await MakeRequestAsync(++ixRequest, failureUrl);
    }


    //
    // Private methods
    //

    private async Task MakeRequestAsync(int ixRequest, string requestUrl)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient(NamedHttpClients.HttpStatusCircuitBreaker.Name);

            _logger.Information("[{IxRequest}] Making HTTP request to {RequestUrl}...", ixRequest, requestUrl);
            using var requestMsg = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            using var responseMsg = await httpClient.SendAsync(requestMsg);

            _logger.Information("[{IxRequest}] Status: {StatusCodeInt} {StatusCode}", ixRequest, (int)responseMsg.StatusCode, responseMsg.StatusCode);
            var responseBody = await responseMsg.Content.ReadAsStringAsync();
            _logger.Information("[{IxRequest}] Response body: {ResponseBody}", ixRequest, responseBody);
        }
        catch (BrokenCircuitException)
        {
            _logger.Error("[{IxRequest}] BrokenCircuitException when making a request to {RequestUrl}", ixRequest, requestUrl);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[{IxRequest}] Unhandled exception when making a request to {RequestUrl}", ixRequest, requestUrl);
        }
    }
}
