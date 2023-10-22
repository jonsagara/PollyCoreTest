using Microsoft.Extensions.Hosting;
using PollyCoreConsole.Infrastructure;
using Serilog;

// The initial "bootstrap" logger is able to log errors during start-up. It's completely replaced by the
//   logger configured in `UseSerilog()`, once configuration and dependency-injection have both been
//   set up successfully.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
.CreateBootstrapLogger();

var builder = Host.CreateApplicationBuilder(args);

// Add services to the container.
ApplicationStartup.ConfigureServices(builder);

try
{
    var app = builder.Build();

    app.Run();

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
