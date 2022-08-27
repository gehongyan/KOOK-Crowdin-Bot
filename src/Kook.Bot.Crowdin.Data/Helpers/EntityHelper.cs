using Crowdin.Api.Glossaries;
using Kook.Bot.Crowdin.Data.Models;

namespace Kook.Bot.Crowdin.Data.Helpers;

public static class EntityHelper
{
    public static TermEntity ToEntity(this Term model) => new()
    {
        Id = model.Id,
        UserId = model.UserId,
        GlossaryId = model.GlossaryId,
        LanguageId = model.LanguageId,
        Text = model.Text,
        Description = model.Description,
        PartOfSpeech = model.PartOfSpeech,
        Lemma = model.Lemma,
        CreatedAt = model.CreatedAt,
        UpdatedAt = model.UpdatedAt,
    };

    public static TermEntity WithTranslations(this TermEntity entity, IEnumerable<Term> translations)
    {
        entity.Translations = translations.Select(x => x.ToEntity()).ToList();
        return entity;
    }
}