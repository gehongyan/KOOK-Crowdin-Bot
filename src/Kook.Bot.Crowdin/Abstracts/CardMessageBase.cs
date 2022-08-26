namespace Kook.Bot.Crowdin.Abstracts;

public abstract class CardMessageBase
{
    public virtual List<ICardBuilder> CardBuilders { get; set; }
    
    public virtual List<ICard> Cards => Build();

    public virtual List<ICard> Build() => CardBuilders.Select(x => x.Build()).ToList();
}