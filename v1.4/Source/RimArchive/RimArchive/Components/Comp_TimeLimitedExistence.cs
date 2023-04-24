using RimWorld;
using System;
using Verse;

namespace RimArchive;
public class CompProperties_ExistedTicks : CompProperties
{
    public int ExistedTicks = 500;
    public CompProperties_ExistedTicks() => compClass = typeof(Comp_TimeLimitedExistence);
}
public class Comp_TimeLimitedExistence : ThingComp

{
    private int ExistedTicks;

    CompProperties_ExistedTicks Props => (CompProperties_ExistedTicks) props;

    public override void Initialize(CompProperties props)
    {
        base.Initialize(props);
        this.ExistedTicks = Props.ExistedTicks;
    }

    public override void CompTick()
    {
        base.CompTick();
        ExistedTicks--;
        if (ExistedTicks <= 0f)
        {
            Messages.Message("Summon_disappered".Translate(this.parent.Label), MessageTypeDefOf.NeutralEvent);
            this.parent.Destroy();
        }
    }


}


