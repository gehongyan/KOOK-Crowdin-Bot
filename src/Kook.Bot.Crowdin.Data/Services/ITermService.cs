using Crowdin.Api.Glossaries;
using Kook.Bot.Crowdin.Data.Models;

namespace Kook.Bot.Crowdin.Data.Services;

public interface ITermService
{
    Task SyncTermsAsync(Dictionary<Term, IEnumerable<Term>> terms);
    
    Task<IReadOnlyCollection<TermEntity>> ListTermsAsync();

    IQueryable<TermEntity> QueryTerms();
}