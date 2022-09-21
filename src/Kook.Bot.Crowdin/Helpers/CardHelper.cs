using Kook.Bot.Crowdin.Cards;

namespace Kook.Bot.Crowdin.Helpers;

public static class CardHelper
{
    public static ParagraphStructBuilder AddFields<T>(this ParagraphStructBuilder builder, IEnumerable<T> fields)
        where T : IElementBuilder
    {
        foreach (T field in fields)
        {
            switch (field)
            {
                case PlainTextElementBuilder plainTextElementBuilder:
                    builder.AddField(plainTextElementBuilder);
                    break;
                case KMarkdownElementBuilder kMarkdownElementBuilder:
                    builder.AddField(kMarkdownElementBuilder);
                    break;
                default:
                    throw new ArgumentException($"{field.GetType()} is not supported");
            }
        }
        return builder;
    }

    public static ICardBuilder AddModules<T>(this ICardBuilder builder, IEnumerable<T> modules)
        where T : IModuleBuilder
    {
        foreach (T module in modules)
            switch (builder)
            {
                case CardBuilder cardBuilder:
                    cardBuilder.AddModule(module);
                    break;
                default:
                    throw new ArgumentException($"{builder.GetType()} is not supported");
            }
        return builder;
    }

    public static string CrowdinLogoUrl => "https://img.kookapp.cn/assets/2022-08/M1dcdZvFb80go0go.png";
    public static string FooterText => $" KOOK Crowdin {Format.Bold("·")} {DateTimeOffset.Now:yyyy'/'M'/'dd HH':'mm':'ss}";
    public static Color CrowdinGreen => new(135, 174, 55);

    public static IEnumerable<IModuleBuilder> FooterModules => new IModuleBuilder[]
    {
        new DividerModuleBuilder(),
        new ContextModuleBuilder()
            .AddElement<ImageElementBuilder>(x => x.Source = CrowdinLogoUrl)
            .AddElement<KMarkdownElementBuilder>(x => x.WithContent(FooterText))
    };
    
    public static CardBuilder GetReferenceStyleFooterCard(this CardBuilder styleReference)
    {
        CardBuilder cardBuilder = new CardBuilder()
            .WithSize(styleReference.Size)
            .WithTheme(styleReference.Theme);
        if (styleReference.Color.HasValue)
            cardBuilder.Color = styleReference.Color.Value;
        return cardBuilder
            .AddModule<ContextModuleBuilder>(x => x
                .AddElement<ImageElementBuilder>(y => y.Source = CrowdinLogoUrl)
                .AddElement<KMarkdownElementBuilder>(y => y
                    .WithContent(FooterText)));
    }

    public static async Task<Cacheable<IUserMessage, Guid>> ReplyInfoCardAsync(
        this IMessage message, IEnumerable<string> content, string title = null, IEnumerable<string> remark = null, bool isQuote = false, bool showFooter = true)
        => await message.Channel.SendCardsAsync(new SimpleInfoCard(content, title, remark, showFooter: showFooter).Cards, 
                quote: isQuote ? new Quote(message.Id) : null)
            .ConfigureAwait(false);
    public static async Task<Cacheable<IUserMessage, Guid>> ReplyInfoCardAsync(
        this IMessage message, string content, string title = null, string remark = null, bool isQuote = false, bool showFooter = true)
        => await message.Channel.SendCardsAsync(new SimpleInfoCard(content, title, remark, showFooter: showFooter).Cards, 
                quote: isQuote ? new Quote(message.Id) : null)
            .ConfigureAwait(false);
    public static async Task<Cacheable<IUserMessage, Guid>> ReplyErrorCardAsync(
        this IMessage message, IEnumerable<string> content, string title = null, IEnumerable<string> remark = null, bool isQuote = false, bool showFooter = true)
        => await message.Channel.SendCardsAsync(new SimpleErrorCard(content, title, remark, showFooter: showFooter).Cards, 
                quote: isQuote ? new Quote(message.Id) : null)
            .ConfigureAwait(false);
    public static async Task<Cacheable<IUserMessage, Guid>> ReplyErrorCardAsync(
        this IMessage message, string content, string title = null, string remark = null, bool isQuote = false, bool showFooter = true)
        => await message.Channel.SendCardsAsync(new SimpleErrorCard(content, title, remark, showFooter: showFooter).Cards, 
                quote: isQuote ? new Quote(message.Id) : null)
            .ConfigureAwait(false);
}