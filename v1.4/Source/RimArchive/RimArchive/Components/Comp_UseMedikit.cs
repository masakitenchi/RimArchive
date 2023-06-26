using RimWorld;
using RimWorld.Planet;
using System;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimArchive;

public class CompProperties_Medkit : CompProperties
{
    public int WoundsToHeal = 1;
    public CompProperties_Medkit() => compClass = typeof(CompMedkit);
}

public class CompMedkit : ThingComp
{
    public CompProperties_Medkit Props => this.props as CompProperties_Medkit;
    public void DoEffect(Pawn pawn)
    {
        if(HealingUtility.TryHealWounds(pawn, out var Message, Props.WoundsToHeal))
        {
            Messages.Message(Message, MessageTypeDefOf.PositiveEvent);
            this.parent.Destroy(DestroyMode.Vanish);
        }
    }
}
public class Medkit : TrapLikeBuilding
{
    protected override void CheckSpring(Pawn p)
    {
        if (p.Faction == Faction.OfPlayer || p.HostFaction == Faction.OfPlayer)
        {
            this.TryGetComp<CompMedkit>().DoEffect(p);
            Messages.Message("Heal Success".Translate(), MessageTypeDefOf.PositiveEvent);
        }
    }
}

/*public class CompProperties_UseMedikit : CompProperties
{
    public CompProperties_UseMedikit() => compClass = typeof(Comp_UseMedikit);
}
public class Comp_UseMedikit : CompUseEffect
{
    public override void DoEffect(Pawn usedBy)
    {
        base.DoEffect(usedBy);
        Log.Message("Step1");
        TaggedString taggedString = HealingUtility.HealWound(usedBy);
    }
}
*/





