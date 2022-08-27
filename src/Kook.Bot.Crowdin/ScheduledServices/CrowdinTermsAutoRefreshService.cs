using System.Diagnostics;
using Crowdin.Api;
using Crowdin.Api.Glossaries;
using Kook.Bot.Crowdin.Configurations;
using Kook.Bot.Crowdin.Data;
using Kook.Bot.Crowdin.Data.Models;
using Kook.Bot.Crowdin.Data.Services;
using Kook.Bot.Crowdin.Helpers;
using Serilog;

namespace Kook.Bot.Crowdin.ScheduledServices;

public class CrowdinTermsAutoRefreshService : ScheduledServiceBase
{
    private readonly ILogger _logger;
    private readonly CrowdinApiClient _crowdinApiClient;
    private readonly CrowdinBotDbContext _dbContext;
    private readonly CrowdinConfigurations _crowdinConfigurations;
    private readonly ITermService _termRepository;
    private readonly SemaphoreSlim _apiSemaphore;

    public CrowdinTermsAutoRefreshService(ILogger logger, 
        CrowdinApiClient crowdinApiClient, 
        CrowdinBotDbContext dbContext, 
        CrowdinConfigurations crowdinConfigurations,
        ITermService termRepository)
        : base(TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(5), logger, nameof(CrowdinTermsAutoRefreshService), false)
    {
        _logger = logger;
        _crowdinApiClient = crowdinApiClient;
        _dbContext = dbContext;
        _crowdinConfigurations = crowdinConfigurations;
        _termRepository = termRepository;
        _apiSemaphore = new SemaphoreSlim(10);
    }

    protected override async Task ExecuteAsync()
    {
        _logger.Information("Crowdin terms refreshing");
        Stopwatch stopwatch = Stopwatch.StartNew();
        IEnumerable<Task<KeyValuePair<Term, IEnumerable<Term>>>> tasks =
            (await ListAllTermsAsync(languageId: _crowdinConfigurations.SourceLanguageId)
                .Select(async x => new KeyValuePair<Term, IEnumerable<Term>>(x,
                    await ListAllTermsAsync(translationOfTermId: x.Id)
                        .Where(y => y.LanguageId != _crowdinConfigurations.SourceLanguageId)
                        .ToListAsync()))
                .ToListAsync())
            .Select(async x => await x);
        KeyValuePair<Term, IEnumerable<Term>>[] results = await Task.WhenAll(tasks);
        Dictionary<Term, IEnumerable<Term>> terms = results.ToDictionary(x => x.Key, x => x.Value);
        stopwatch.Stop();
        _logger.Information("{Count} terms found in {ElapsedMilliseconds}ms", terms.Count,
            stopwatch.ElapsedMilliseconds);
        stopwatch.Restart();
        await _termRepository.SyncTermsAsync(terms);
        _logger.Information("Update database in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
    }

    private async IAsyncEnumerable<Term> ListAllTermsAsync(string languageId = null, int? translationOfTermId = null)
    {
        Pagination pagination = new() {Limit = 500, Offset = 0};
        ResponseList<Term> responseList;
        do
        {
            await _apiSemaphore.WaitAsync();
            try
            {
                _logger.Verbose("Listing terms in {LanguageId} for {TranslationOfTermId}, Limit: {Limit}, Offset: {Offset}", 
                    languageId, translationOfTermId, pagination.Limit, pagination.Offset);
                responseList = await _crowdinApiClient.Glossaries
                    .ListTerms(_crowdinConfigurations.GlossaryId, languageId: languageId,
                        translationOfTermId: translationOfTermId, limit: pagination.Limit, offset: pagination.Offset);
                _logger.Verbose("Listed terms in {LanguageId} for {TranslationOfTermId}, Limit: {Limit}, Offset: {Offset}, Count: {Count}", 
                    languageId, translationOfTermId, pagination.Limit, pagination.Offset, responseList.Data.Count);
                pagination = responseList.Pagination;
                pagination.Offset += pagination.Limit;
            }
            finally
            {
                _apiSemaphore.Release();
            }
            foreach (Term term in responseList.Data)
            {
                yield return term;
            }
        } while (responseList.Data.Count == pagination.Limit);
    }
}