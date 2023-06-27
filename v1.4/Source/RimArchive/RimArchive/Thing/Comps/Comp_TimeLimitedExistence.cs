namespace RimArchive;
public class CompProperties_ExistedTicks : CompProperties
{
    public int ExistedTicks = 500;
    public CompProperties_ExistedTicks() => compClass = typeof(Comp_TimeLimitedExistence);
}
public class Comp_TimeLimitedExistence : ThingComp

{
    private int _ticksLeft;

    CompProperties_ExistedTicks Props => (CompProperties_ExistedTicks)props;

    public override void Initialize(CompProperties props)
    {
        base.Initialize(props);
        this._ticksLeft = Props.ExistedTicks;
    }

    public override void CompTick()
    {
        _ticksLeft--;
        if (_ticksLeft <= 0)
        {
            Messages.Message("Summon_disappered".Translate(this.parent.Label), MessageTypeDefOf.NeutralEvent);
            this.parent.Destroy();
        }
    }

    public override string CompInspectStringExtra()
    {
        return GenTicks.ToStringSecondsFromTicks(this._ticksLeft);
    }

}


