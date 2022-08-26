using Crowdin.Api;
using Crowdin.Api.Glossaries;
using Crowdin.Api.ProjectsGroups;
using Crowdin.Api.Users;
using Kook.Bot.Crowdin.Cards;
using Kook.Bot.Crowdin.Data.Models;
using Kook.Bot.Crowdin.Data.Services;
using Kook.Bot.Crowdin.Helpers;
using Kook.Bot.Crowdin.ScheduledServices;
using Kook.Commands;

namespace Kook.Bot.Crowdin.Modules;

[Group("glossary")]
[Alias("glossaries", "词汇", "词汇表", "术语", "术语表")]
public class GlossaryModule : ModuleBase<SocketCommandContext>
{
    private readonly CrowdinApiClient _crowdinApiClient;
    private readonly CrowdinTermsAutoRefreshService _crowdinTermsAutoRefreshService;
    private readonly ITermService _termRepository;

    public GlossaryModule(CrowdinApiClient crowdinApiClient,
        CrowdinTermsAutoRefreshService crowdinTermsAutoRefreshService,
        ITermService termRepository)
    {
        _crowdinApiClient = crowdinApiClient;
        _crowdinTermsAutoRefreshService = crowdinTermsAutoRefreshService;
        _termRepository = termRepository;
    }

    [Command("refresh")]
    [Alias("更新", "刷新")]
    public async Task ForceRefresh()
    {
        _crowdinTermsAutoRefreshService.ExecuteImmediately();
        await ReplyTextAsync("已手动触发词汇表刷新任务");
    }
    
    [Command("list")]
    [Alias("列表")]
    public async Task ListAll()
    {
        IReadOnlyCollection<TermEntity> terms = await _termRepository.ListTermsAsync();
        // await ReplyCardsAsync(new TermListCard(terms).Cards, isQuote: true);
        Stream stream = await terms.ToList().ToExcelAsync();
        // await ReplyCardsAsync(new CardBuilder().AddModule<FileModuleBuilder>(x => x.WithTitle("terms.xlsx").WithSource()))
        await ReplyFileAsync(stream, "terms.xlsx", isQuote: true);
    }
}