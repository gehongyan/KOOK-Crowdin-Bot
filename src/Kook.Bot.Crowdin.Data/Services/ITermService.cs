using Crowdin.Api.Glossaries;
using Kook.Bot.Crowdin.Data.Models;

namespace Kook.Bot.Crowdin.Data.Services;

public interface ITermService
{
    Task SyncTermsAsync(IEnumerable<IGrouping<Term, Term>> terms);
    
    Task<IReadOnlyCollection<TermEntity>> ListTermsAsync();
}