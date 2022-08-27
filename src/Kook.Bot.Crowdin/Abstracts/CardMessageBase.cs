namespace Kook.Bot.Crowdin.Abstracts;

public abstract class CardMessageBase
{
    protected abstract IEnumerable<ICardBuilder> CardBuilders { get; }
    
    public virtual IEnumerable<ICard> Cards => Build();

    protected virtual List<ICard> Build() => CardBuilders.Select(x => x.Build()).ToList();
}