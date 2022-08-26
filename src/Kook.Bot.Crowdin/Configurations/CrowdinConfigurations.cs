using Kook.Bot.Crowdin.Attributes;
using Kook.Bot.Crowdin.Interfaces;

namespace Kook.Bot.Crowdin.Configurations;

[Configurations]
public class CrowdinConfigurations : IConfigurations
{
    /// <summary>
    ///     Crowdin 接口密钥
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    ///     术语库ID
    /// </summary>
    public int GlossaryId { get; set; }

    /// <summary>
    ///     源语言
    /// </summary>
    public string SourceLanguageId { get; set; }

    /// <summary>
    ///     目标语言
    /// </summary>
    public IReadOnlyCollection<string> TargetLanguageIds { get; set; }
}