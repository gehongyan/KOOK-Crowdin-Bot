using Kook.Bot.Crowdin.Configurations;
using Kook.WebSocket;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Kook.Bot.Crowdin.Extensions;

public partial class KookBotExtension : IHostedService
{
    private readonly KookConfigurations _kookConfigurations;
    private readonly KookSocketClient _kookSocketClient;
    private readonly KookCommandHandlingExtension _kookCommandHandlingExtension;
    private readonly ILogger _logger;

    public KookBotExtension(
        KookConfigurations kookConfigurations,
        KookSocketClient kookSocketClient,
        KookCommandHandlingExtension kookCommandHandlingExtension,
        ILogger logger)
    {
        _kookConfigurations = kookConfigurations;
        _kookSocketClient = kookSocketClient;
        _kookCommandHandlingExtension = kookCommandHandlingExtension;
        _logger = logger;
        
        _kookSocketClient.Log += LogHandler;
        _kookSocketClient.Ready += KookSocketClientOnReady;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _kookSocketClient.LoginAsync(TokenType.Bot, _kookConfigurations.Token);
        await _kookSocketClient.StartAsync();
        await _kookCommandHandlingExtension.InitializeAsync();
    }
    
    /// <summary>
    ///     KaiHeiLa.Net日志事件处理程序
    /// </summary>
    private Task LogHandler(LogMessage msg)
    {
        switch (msg.Severity)
        {
            case LogSeverity.Critical:
                _logger.Fatal("KaiHeiLa {Message:l}", msg.ToString());
                break;
            case LogSeverity.Error:
                _logger.Error("KaiHeiLa {Message:l}", msg.ToString());
                break;
            case LogSeverity.Warning:
                _logger.Warning("KaiHeiLa {Message:l}", msg.ToString());
                break;
            case LogSeverity.Info:
                _logger.Information("KaiHeiLa {Message:l}", msg.ToString());
                break;
            case LogSeverity.Verbose:
                _logger.Debug("KaiHeiLa {Message:l}", msg.ToString());
                break;
            case LogSeverity.Debug:
                _logger.Verbose("KaiHeiLa {Message:l}", msg.ToString());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(LogSeverity));
        }

        return Task.CompletedTask;
    }
    
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _kookSocketClient.LogoutAsync();
        _logger.Fatal("KaiHeiLa Client Stopped!");
    }
}