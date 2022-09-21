using Serilog;
using Serilog.Configuration;

namespace Kook.Bot.Crowdin.Extensions;

public static class SerilogKookSinkExtension
{
    public static LoggerConfiguration KookChannelSink(
        this LoggerSinkConfiguration loggerConfiguration,
        IServiceProvider services)
    {
        return loggerConfiguration.Sink(new SerilogKookSink(services));
    }
}