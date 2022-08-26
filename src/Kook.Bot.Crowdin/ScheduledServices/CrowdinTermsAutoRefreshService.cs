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
        : base(TimeSpan.Zero, TimeSpan.FromMinutes(5), logger, nameof(CrowdinTermsAutoRefreshService), false)
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
        IAsyncEnumerable<Term> sourceTerms = ListAllTermsAsync(languageId: _crowdinConfigurations.SourceLanguageId);
        List<IGrouping<Term, Term>> groups = new();
        await foreach (Term sourceTerm in sourceTerms)
        {
            List<Term> translations = await ListAllTermsAsync(translationOfTermId: sourceTerm.Id)
                .Where(x => x.LanguageId != _crowdinConfigurations.SourceLanguageId)
                .ToListAsync();
            groups.Add(new Grouping<Term, Term>(sourceTerm, translations));
        }
        await _termRepository.SyncTermsAsync(groups);
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
                _logger.Information("Listing terms in {LanguageId} for {TranslationOfTermId}, Limit: {Limit}, Offset: {Offset}", 
                    languageId, translationOfTermId, pagination.Limit, pagination.Offset);
                responseList = await _crowdinApiClient.Glossaries
                    .ListTerms(_crowdinConfigurations.GlossaryId, languageId: languageId,
                        translationOfTermId: translationOfTermId, limit: pagination.Limit, offset: pagination.Offset);
                _logger.Information("Listed terms in {LanguageId} for {TranslationOfTermId}, Limit: {Limit}, Offset: {Offset}, Count: {Count}", 
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