using Kook.Bot.Crowdin.Abstracts;
using Kook.Bot.Crowdin.Data.Models;

namespace Kook.Bot.Crowdin.Cards;

public sealed class TermListCard : CardMessageBase
{
    public TermListCard(IReadOnlyCollection<TermEntity> terms)
    {
        CardBuilders = GenerateCardBuilder(terms);
    }

    private static List<ICardBuilder> GenerateCardBuilder(IReadOnlyCollection<TermEntity> terms)
    {
        string GetTranslationDescription(TermEntity term)
        {
            return term.Translations.Any() 
                ? term.Translations
                    .Select(x => $" **[{x.LanguageId}]** {x.Text}")
                    .Aggregate((a, b) => $"{a}{b}") 
                : "-";
        }
        IEnumerable<string> contents = terms
            .Select(x => $"{x.Text}{GetTranslationDescription(x)}")
            .Chunk(100)
            .Select(x => x
                .Aggregate((a, b) => $"{a}\n{b}"));
        IEnumerable<ICardBuilder> cardBuilders = contents
            .Select(x => new CardBuilder()
                .WithSize(CardSize.Large)
                .AddModule<SectionModuleBuilder>(y => y
                    .WithText<KMarkdownElementBuilder>(z => z
                        .WithContent(x))));
        return cardBuilders.ToList();
    }
}