using Kook.Bot.Crowdin.Abstracts;
using Kook.Bot.Crowdin.Helpers;

namespace Kook.Bot.Crowdin.Cards;

public class SimpleErrorCard : CardMessageBase
{
    private readonly IEnumerable<string> _content;
    private readonly string _title;
    private readonly IEnumerable<string> _remark;
    private readonly bool _showFooter;

    public SimpleErrorCard(IEnumerable<string> content, string title = null, IEnumerable<string> remark = null,
        bool showFooter = false)
    {
        _content = content?.Where(c => !string.IsNullOrWhiteSpace(c)) ?? Enumerable.Empty<string>();
        _title = title;
        _remark = remark?.Where(c => !string.IsNullOrWhiteSpace(c)) ?? Enumerable.Empty<string>();
        _showFooter = showFooter;
    }

    public SimpleErrorCard(string content, string title = null, string remark = null, bool showFooter = false)
    {
        _content = new []{content ?? string.Empty}.Where(c => !string.IsNullOrWhiteSpace(c));
        _title = title;
        _remark =  new []{remark ?? string.Empty}.Where(c => !string.IsNullOrWhiteSpace(c));
        _showFooter = showFooter;
    }
    
    protected override IEnumerable<ICardBuilder> CardBuilders => GenerateCardBuilders(_content, _title, _remark, _showFooter);

    private static IEnumerable<ICardBuilder> GenerateCardBuilders(IEnumerable<string> content, string title,
        IEnumerable<string> remark, bool showFooter)
    {
        CardBuilder card = new CardBuilder()
            .WithSize(CardSize.Large)
            .WithColor(Color.Red);
        if (!string.IsNullOrWhiteSpace(title))
            card.AddModule<HeaderModuleBuilder>(x => x.Text = title);
        card.AddModules(content.Select(x => new SectionModuleBuilder().WithText(x, true)));
        List<string> remarkList = remark.ToList();
        if (remarkList.Any())
            card.AddModule<DividerModuleBuilder>()
                .AddModule(remarkList.Select(x => new KMarkdownElementBuilder().WithContent(x))
                .Aggregate(new ContextModuleBuilder(), (x, y) => x.AddElement(y)));
        if (showFooter)
            card.AddModules(CardHelper.FooterModules);
        return new[] {card};
    }
}