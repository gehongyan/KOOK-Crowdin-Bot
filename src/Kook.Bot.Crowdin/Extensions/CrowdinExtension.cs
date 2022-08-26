using Crowdin.Api;

namespace Kook.Bot.Crowdin.Extensions;

public class CrowdinExtension
{
    private readonly CrowdinApiClient _crowdinApiClient;

    public CrowdinExtension(CrowdinApiClient crowdinApiClient)
    {
        _crowdinApiClient = crowdinApiClient;
    }
    
    
}