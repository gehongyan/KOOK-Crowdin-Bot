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
    //
    // public async Task SyncTermsAsync(IEnumerable<IGrouping<Term, Term>> terms)
    // {
    //     List<TermEntity> entities = await _dbContext.Terms.ToListAsync();
    //     List<IGrouping<Term,Term>> groups = terms.ToList();
    //     foreach (IGrouping<Term, Term> group in groups)
    //     {
    //         TermEntity termEntity = group.Key.ToEntity();
    //         termEntity.Translations = group.Select(x => x.ToEntity()).ToList();
    //         if (entities.Any(x => x.Id == termEntity.Id))
    //         {
    //             // _dbContext.Entry(entities.Single(x => x.Id == termEntity.Id)).CurrentValues.SetValues(termEntity);
    //             // termEntity.Translations.ForEach(x => _dbContext.Entry(entities.Single(y => y.Id == termEntity.Id)).CurrentValues.SetValues(termEntity));
    //             _dbContext.Entry(termEntity).State = EntityState.Modified;
    //             termEntity.Translations.ForEach(x => _dbContext.Entry(x).State = EntityState.Modified);
    //         }
    //         else
    //             await _dbContext.Terms.AddAsync(termEntity);
    //     }
    //     List<int> existedItemIds = groups.Select(x => x.Key.Id).ToList();
    //     existedItemIds.AddRange(groups.SelectMany(x => x.Select(y => y.Id)));
    //     entities.ExceptBy(existedItemIds, x => x.Id).ToList()
    //         .ForEach(x => _dbContext.Entry(x).State = EntityState.Deleted);
    //     await _dbContext.SaveChangesAsync();
    // }
    //
    //
    public async Task SyncTermsAsync(IEnumerable<IGrouping<Term, Term>> terms)
    {
        List<TermEntity> entities = await _dbContext.Terms.AsNoTrackingWithIdentityResolution().ToListAsync();
        List<IGrouping<Term,Term>> groups = terms.ToList();
        List<TermEntity> entitiesNeedToDetach = new();
        foreach (IGrouping<Term, Term> group in groups)
        {
            TermEntity termEntity = group.Key.ToEntity();
            termEntity.Translations = group.Select(x => x.ToEntity()).ToList();
            entitiesNeedToDetach.Add(termEntity);
            entitiesNeedToDetach.AddRange(termEntity.Translations);
            if (entities.Any(x => x.Id == termEntity.Id))
            {
                _dbContext.Entry(termEntity).State = EntityState.Modified;
                termEntity.Translations.ForEach(x => _dbContext.Entry(x).State = EntityState.Modified);
            }
            else
                _dbContext.Entry(termEntity).State = EntityState.Added;
        }
        List<int> existedItemIds = groups.Select(x => x.Key.Id).ToList();
        existedItemIds.AddRange(groups.SelectMany(x => x.Select(y => y.Id)));
        entities.ExceptBy(existedItemIds, x => x.Id).ToList()
            .ForEach(x => _dbContext.Entry(x).State = EntityState.Deleted);
        await _dbContext.SaveChangesAsync();
        foreach (TermEntity termEntity in entitiesNeedToDetach)
            _dbContext.Entry(termEntity).State = EntityState.Detached;
    }

    public async Task<IReadOnlyCollection<TermEntity>> ListTermsAsync() =>
        await _dbContext.Terms
            .AsNoTracking()
            .Include(x => x.Translations)
            .Where(x => x.ParentEntityId == null)
            .ToListAsync();
}