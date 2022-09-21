using Kook.Bot.Crowdin.Cards;
using Kook.Commands;

namespace Kook.Bot.Crowdin.Modules;

[Group("help")]
[Alias("h", "?", "帮助")]
public class HelpModule : ModuleBase<SocketCommandContext>
{
    [Command("")]
    [Alias("summary", "概述")]
    public async Task Help(string targetMessageId = null, bool quote = true)
    {
        IUserMessage targetMessage = null;
        if (Guid.TryParse(targetMessageId, out Guid id))
            if (await Context.Channel.GetMessageAsync(id).ConfigureAwait(false) is IUserMessage message)
                targetMessage = message;
        
        IEnumerable<ICard> cards = new SimpleInfoCard(
            @"`!glossary export` 导出术语词汇表
`!glossary language <lang_id>` 单一目标语言术语词汇对照表
`!glossary search <keyword>` 搜索术语", 
            "术语", 
            @"**·** 命令由 `!` 引导
**·** `<>` 所围成的部分为参数
**·** 每个命令都在一定程度上支持同义词与中文翻译调用",
            showFooter: true).Cards;
        
        if (targetMessage is not null)
            await targetMessage.ModifyAsync(x => x.Cards = cards.Select(y => (Card) y).ToList());
        else
            await Context.Channel.SendCardsAsync(cards.Select(y => (Card) y).ToList(), 
                quote: quote ? new Quote(Context.Message.Id) : null);
    }
}