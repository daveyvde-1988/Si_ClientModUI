namespace SiSquadHud;
internal sealed class RevisionCache<T>
{
    internal long Revision {get;private set;}=-1;
    private bool ready;
    private double next;
    private T value;
    internal void Force()=>next=0;
    internal T Get(double now,long revision,Func<T> build)
    {
        if(!ready||revision!=Revision&&now>=next)
        {value=build();ready=true;Revision=revision;next=now+UiPolicies.UiRefreshSeconds;}
        return value;
    }
}
