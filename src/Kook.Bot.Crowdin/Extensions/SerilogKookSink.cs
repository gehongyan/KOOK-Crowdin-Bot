using Kook.Bot.Crowdin.Cards;
using Kook.Bot.Crowdin.Configurations;
using Kook.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using Serilog.Events;

namespace Kook.Bot.Crowdin.Extensions;

public class SerilogKookSink : ILogEventSink
{
    private readonly KookConfigurations _kookConfigurations;
    private readonly KookSocketClient _client;
    private IMessageChannel _logChannel;

    public SerilogKookSink(IServiceProvider services)
    {
        _kookConfigurations = services.GetRequiredService<KookConfigurations>();
        _client = services.GetRequiredService<KookSocketClient>();
    }
    
    public void Emit(LogEvent logEvent)
    {
        Task.Run(() =>
        {
            try
            {
                _logChannel ??= _client.GetChannel(_kookConfigurations.LogChannelId) as IMessageChannel;
                if (_logChannel is null) return;

                IEnumerable<ICard> cards = new SimpleInfoCard(Format.Sanitize(logEvent.RenderMessage())).Cards;
                _logChannel.SendCardsAsync(cards);
            }
            catch
            {
                // ignored
            }
        });
    }
}