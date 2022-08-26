using System.ComponentModel.DataAnnotations.Schema;
using Crowdin.Api.Glossaries;

namespace Kook.Bot.Crowdin.Data.Models;

public class TermEntity
{
    public TermEntity()
    {
        Translations = new List<TermEntity>();
    }
    
    public int Id { get; set; }
    
    public int UserId { get; set; }

    public int GlossaryId { get; set; }

    public string LanguageId { get; set; }

    public string Text { get; set; }

    public string Description { get; set; }

    public PartOfSpeech PartOfSpeech { get; set; }

    public string Lemma { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public int? ParentEntityId { get; set; }
    
    public TermEntity ParentEntity { get; set; }
    
    public List<TermEntity> Translations { get; set; }
}