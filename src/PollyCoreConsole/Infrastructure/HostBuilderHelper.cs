using System.Reflection;
using Microsoft.Extensions.Hosting;
using Sagara.Core.Logging.Serilog;
using Serilog;

namespace PollyCoreConsole.Infrastructure;

public static class HostBuilderHelper
{
    public static void ConfigureSerilog(IHostApplicationBuilder builder, IServiceProvider services, LoggerConfiguration loggerConfig)
    {
        // This is the .exe path in bin/{configuration}/{tfm}/
        var currentExeDir = builder.Environment.ContentRootPath;

        // Log to the project directory.
        var logDir = Path.GetFullPath(Path.Combine(currentExeDir, @"..\..\.."));
        Log.Logger.Information("Logging directory: {logDir}", logDir);

        var logFilePathFormat = Path.Combine(logDir, "Logs", "log.txt");


        //
        // Configure Serilog and its various sinks.
        //

        // Always write to a rolling file. For bad API request logging ONLY, write to a special file.
        loggerConfig = loggerConfig
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.WithProperty(LoggerEnrichmentProperties.Assembly, Assembly.GetExecutingAssembly().GetName().Name!)
            .Enrich.With<UtcTimestampEnricher>()
            .Enrich.WithMachineName()
            .WriteTo.Console()
            .WriteTo.Logger(lc => lc
                .WriteTo.File(logFilePathFormat, outputTemplate: "{UtcTimestamp:yyyy-MM-dd HH:mm:ss.fff} [{MachineName}] [{Level}] [{SourceContext:l}] {Message}{NewLine}{Exception}", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 10)
                );
    }
}
