using Microsoft.Extensions.Hosting;
using Sagara.Core.Logging.Serilog;
using Serilog;

namespace PollyCoreConsole.Infrastructure;

public static class ApplicationStartup
{
    public static void ConfigureServices(HostApplicationBuilder builder)
    {
        builder.UseSerilog(HostBuilderHelper.ConfigureSerilog);

        
        //
        // Register HttpClients
        //

        //builder.Services.AddHttpClient("Foo")
        //    .AddTransientHttpErrorPolicy(PollyHelper.BuildWaitAndRetryPolicy);
    }
}
