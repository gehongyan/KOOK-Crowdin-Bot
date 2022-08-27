using Kook.Bot.Crowdin.Abstracts;
using Kook.Bot.Crowdin.Data.Models;
using Kook.Bot.Crowdin.Helpers;

namespace Kook.Bot.Crowdin.Cards;

public class QueryTermsCard : CardMessageBase
{
    private readonly string _keyword;
    private readonly IEnumerable<TermEntity> _terms;

    public QueryTermsCard(string keyword, IEnumerable<TermEntity> terms)
    {
        _keyword = keyword;
        _terms = terms;
    }

    protected override IEnumerable<ICardBuilder> CardBuilders => GenerateCardBuilders(_keyword, _terms);

    private static IEnumerable<ICardBuilder> GenerateCardBuilders(string keyword, IEnumerable<TermEntity> terms)
    {
        CardBuilder cardBuilder = new CardBuilder().WithSize(CardSize.Large).WithColor(CardHelper.CrowdinGreen)
            .AddModule<HeaderModuleBuilder>(x => x.WithText($"搜索 - {keyword}"));
        IEnumerable<IModuleBuilder> moduleBuilders = terms.SelectMany(x =>
        {
            IEnumerable<IModuleBuilder> builder = new List<IModuleBuilder>
            {
                new DividerModuleBuilder(),
                new HeaderModuleBuilder().WithText(x.Text),
                new SectionModuleBuilder().WithText<ParagraphStructBuilder>(y => y
                    .WithColumnCount(2)
                    .AddFields(x.Translations
                        .Select(z => $"{Format.Bold(z.LanguageId)}\n{z.Text.Replace(keyword, Format.Bold(keyword))}")
                        .Select(z => new KMarkdownElementBuilder().WithContent(z))))
            };
            return builder;
        });
        cardBuilder.AddModules(moduleBuilders);
        return new[] {cardBuilder};
    }
}