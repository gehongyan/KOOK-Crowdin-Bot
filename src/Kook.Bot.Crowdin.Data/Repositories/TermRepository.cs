using Crowdin.Api.Glossaries;
using Kook.Bot.Crowdin.Data.Helpers;
using Kook.Bot.Crowdin.Data.Models;
using Kook.Bot.Crowdin.Data.Services;
using Microsoft.EntityFrameworkCore;

namespace Kook.Bot.Crowdin.Data.Repositories;

public class TermRepository : ITermService
{
    private readonly CrowdinBotDbContext _dbContext;

    public TermRepository(CrowdinBotDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task SyncTermsAsync(Dictionary<Term, IEnumerable<Term>> terms)
    {
        _dbContext.Terms.RemoveRange(_dbContext.Terms);
        foreach ((Term source, IEnumerable<Term> translations) in terms)
            _dbContext.Add(source.ToEntity().WithTranslations(translations));
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<TermEntity>> ListTermsAsync() =>
        await _dbContext.Terms
            .AsNoTracking()
            .Include(x => x.Translations)
            .Where(x => x.ParentEntityId == null)
            .ToListAsync();

    public IQueryable<TermEntity> QueryTerms() =>
        _dbContext.Terms.AsQueryable();
}