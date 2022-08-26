using Kook.Bot.Crowdin.Attributes;
using Kook.Bot.Crowdin.Interfaces;

namespace Kook.Bot.Crowdin.Configurations;

[Configurations]
public class KookConfigurations : IConfigurations
{
    /// <summary>
    ///     KOOK 接口密钥
    /// </summary>
    public string Token { get; set; }
}