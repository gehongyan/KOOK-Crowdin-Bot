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

    public static Color CrowdinGreen => new(135, 174, 55);
}