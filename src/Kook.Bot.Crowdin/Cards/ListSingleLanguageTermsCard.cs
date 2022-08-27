using Kook.Bot.Crowdin.Abstracts;
using Kook.Bot.Crowdin.Data.Models;
using Kook.Bot.Crowdin.Helpers;

namespace Kook.Bot.Crowdin.Cards;

public sealed class ListSingleLanguageTermsCard : CardMessageBase
{
    private readonly string _sourceLanguageId;
    private readonly string _targetLanguageId;
    private readonly IEnumerable<TermEntity> _terms;

    public ListSingleLanguageTermsCard(
        string sourceLanguageId, 
        string targetLanguageId, 
        IEnumerable<TermEntity> terms)
    {
        _sourceLanguageId = sourceLanguageId;
        _targetLanguageId = targetLanguageId;
        _terms = terms;
    }

    protected override IEnumerable<ICardBuilder> CardBuilders => GenerateCardBuilders(_sourceLanguageId, _targetLanguageId, _terms);
    
    private static IEnumerable<ICardBuilder> GenerateCardBuilders(
        string sourceLanguageId, 
        string targetLanguageId, 
        IEnumerable<TermEntity> terms)
    {
        List<string> innerTexts = new(){Format.Bold(sourceLanguageId), Format.Bold(sourceLanguageId)};
        innerTexts.AddRange(terms.SelectMany(x => new List<string>
            {x.Text, x.Translations.Single(y => y.LanguageId == targetLanguageId).Text}));
        List<ICardBuilder> cardBuilders = innerTexts
            .Chunk(50)
            .Select((chunk, index) =>
            {
                CardBuilder cardBuilder = new CardBuilder().WithSize(CardSize.Large).WithColor(CardHelper.CrowdinGreen);
                if (index == 0)
                    cardBuilder.AddModule<HeaderModuleBuilder>(x => x.WithText("术语表")).AddModule<DividerModuleBuilder>();
                cardBuilder.AddModule<SectionModuleBuilder>(x => x.WithText<ParagraphStructBuilder>(y =>
                    y.WithColumnCount(2)
                        .AddFields(chunk.Select(z => new KMarkdownElementBuilder().WithContent(z)))));
                return (ICardBuilder) cardBuilder;
            }).ToList();
        return cardBuilders;
    }

}