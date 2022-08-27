using Crowdin.Api;
using Crowdin.Api.Glossaries;
using Crowdin.Api.ProjectsGroups;
using Crowdin.Api.Users;
using Kook.Bot.Crowdin.Cards;
using Kook.Bot.Crowdin.Configurations;
using Kook.Bot.Crowdin.Data.Models;
using Kook.Bot.Crowdin.Data.Services;
using Kook.Bot.Crowdin.Helpers;
using Kook.Bot.Crowdin.ScheduledServices;
using Kook.Commands;
using Microsoft.EntityFrameworkCore;

namespace Kook.Bot.Crowdin.Modules;

[Group("glossary")]
[Alias("glossaries", "term", "terms", "词汇", "词汇表", "术语", "术语表")]
public class GlossaryModule : ModuleBase<SocketCommandContext>
{
    private readonly CrowdinApiClient _crowdinApiClient;
    private readonly CrowdinConfigurations _crowdinConfigurations;
    private readonly CrowdinTermsAutoRefreshService _crowdinTermsAutoRefreshService;
    private readonly ITermService _termRepository;

    public GlossaryModule(CrowdinApiClient crowdinApiClient,
        CrowdinConfigurations crowdinConfigurations,
        CrowdinTermsAutoRefreshService crowdinTermsAutoRefreshService,
        ITermService termRepository)
    {
        _crowdinApiClient = crowdinApiClient;
        _crowdinConfigurations = crowdinConfigurations;
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
        Stream stream = await terms.ToList().ToExcelAsync();
        await ReplyFileAsync(stream, $"Terms-{DateTimeOffset.Now:yyyyMMddHHmmss}.xlsx", isQuote: true);
    }

    [Command("language")]
    [Alias("lang", "语言")]
    public async Task ListSingleLanguage(string targetLanguageId)
    {
        IReadOnlyCollection<TermEntity> entities = await _termRepository.ListTermsAsync();
        IEnumerable<TermEntity> termsWithTargetLanguage = entities.Where(x => x.Translations
            .Any(y => y.LanguageId == targetLanguageId)).ToList();
        
        if (termsWithTargetLanguage.Any())
        {
            IEnumerable<ICard> cards = new ListSingleLanguageTermsCard(_crowdinConfigurations.SourceLanguageId, targetLanguageId,
                termsWithTargetLanguage).Cards;
            await ReplyCardsAsync(cards, isQuote: true);
            return;
        }

        await ReplyKMarkdownAsync($"目标语言 {targetLanguageId} 无词汇记录");
    }

    [Command("query")]
    [Alias("search", "find", "查询", "查找", "搜索")]
    public async Task QueryTerms([Remainder] string keyword)
    {
        IReadOnlyCollection<TermEntity> entities = await _termRepository.ListTermsAsync();
        IEnumerable<TermEntity> terms = entities
            .Where(x => x.Text.Contains(keyword)
                        || x.Translations.Any(y => y.Text.Contains(keyword)))
            .OrderByDescending(x =>
            {
                IEnumerable<TermEntity> translations = x.Translations
                    .Where(y => y.Text.Contains(keyword))
                    .ToList();
                int minLength = translations.Any()
                    ? translations.Min(y => y.Text.Length)
                    : int.MaxValue;
                if (x.Text.Contains(keyword) && x.Text.Length < minLength) minLength = x.Text.Length;
                return minLength;
            }).Take(5).ToList();
        if (terms.Any())
        {
            IEnumerable<ICard> cards = new QueryTermsCard(keyword, terms).Cards;
            await ReplyCardsAsync(cards, isQuote: true);
            return;
        }
        await ReplyKMarkdownAsync($"关键字 {keyword} 无匹配结果");
    }
    
}