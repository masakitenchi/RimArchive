using RimWorld;
using Verse;

namespace RimArchive;

public class CompStunHandler : ThingComp
{
    public CompProperties_StunHandler Props => (CompProperties_StunHandler)props;

    //60Tick/s, 250Tick/TickRare, 2000Tick/TickLong
    public override void CompTickRare()
    {
        base.CompTickRare();
        Props.currentDuration -= 30f;
    }
}
