using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PollyCoreConsole;
using PollyCoreConsole.Infrastructure;
using PollyCoreConsole.Services;
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

        // Retry demo
        //var retrySvc = services.GetRequiredService<RetryService>();
        //await retrySvc.RetryRequestDemoAsync("https://httpstat.us/500");
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
