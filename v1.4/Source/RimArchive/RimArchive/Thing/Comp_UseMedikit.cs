using System;

namespace RimArchive;

[Obsolete("Use CompProperties_Healing instead")]
public class CompProperties_Medkit : CompProperties
{
    public int WoundsToHeal = 1;
    public CompProperties_Medkit() => compClass = typeof(CompMedkit);
}

[Obsolete("Use CompProperties_Healing instead")]
public class CompMedkit : ThingComp
{
    public CompProperties_Medkit Props => this.props as CompProperties_Medkit;
    public void DoEffect(Pawn pawn)
    {
        if (HealingUtility.TryHealWounds(pawn, out var Message, Props.WoundsToHeal))
        {
            Messages.Message(Message, MessageTypeDefOf.PositiveEvent);
            this.parent.Destroy(DestroyMode.Vanish);
        }
    }
}





