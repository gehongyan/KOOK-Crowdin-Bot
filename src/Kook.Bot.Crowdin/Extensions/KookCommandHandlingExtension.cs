using System.Reflection;
using Kook.Commands;
using Kook.WebSocket;

namespace Kook.Bot.Crowdin.Extensions
{
    public class KookCommandHandlingExtension
    {
        private readonly IServiceProvider _services;
        private readonly CommandService _commandService;
        private readonly KookSocketClient _kookSocketClient;

        public KookCommandHandlingExtension(
            IServiceProvider services, 
            CommandService commandService, 
            KookSocketClient kookSocketClient)
        {
            _services = services;
            _commandService = commandService;
            _kookSocketClient = kookSocketClient;

            _commandService.CommandExecuted += CommandExecutedAsync;
            _kookSocketClient.MessageReceived += MessageReceivedAsync;
            _kookSocketClient.DirectMessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync()
        {
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            if (rawMessage is not SocketUserMessage {Source: MessageSource.User} message) return;
            int argPos = 0;
            if (!message.HasCharPrefix('!', ref argPos)) return;
            SocketCommandContext context = new(_kookSocketClient, message);
            await _commandService.ExecuteAsync(context, argPos, _services);
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified) return;
            if (result.IsSuccess) return;
            await context.Channel.SendTextMessageAsync($"error: {result}");
        }
    }
}
