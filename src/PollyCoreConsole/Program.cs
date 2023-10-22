using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PollyCoreConsole;
using PollyCoreConsole.Infrastructure;
using Serilog;

// The initial "bootstrap" logger is able to log errors during start-up. It's completely replaced by the
//   logger configured in `UseSerilog()`, once configuration and dependency-injection have both been
//   set up successfully.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    // Add services to the container.
    ApplicationStartup.ConfigureServices(builder);

    var app = builder.Build();

    using (var serviceScope = app.Services.CreateScope())
    {
        var services = serviceScope.ServiceProvider;

        var httpClientFactory = services.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient(HttpClients.HttpStatus);

        var requestUrl = "https://httpstat.us/503";

        Log.Information($"Making HTTP request to {requestUrl}...");
        using var requestMsg = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        using var responseMsg = await httpClient.SendAsync(requestMsg);

        Log.Information($"Status: {responseMsg.StatusCode}");
        var responseBody = await responseMsg.Content.ReadAsStringAsync();
        Log.Information("Response body: {ResponseBody}", responseBody);
    }

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly.");
    return -1;
}
finally
{
    Log.CloseAndFlush();
}
