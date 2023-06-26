using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RimArchive;

public class Medkit : TrapLikeBuilding
{
    protected override void CheckSpring(Pawn p)
    {
        if (p.Faction == Faction.OfPlayer || p.HostFaction == Faction.OfPlayer)
        {
            this.TryGetComp<CompHealing>().DoEffect(p);
            Messages.Message("Heal Success".Translate(), MessageTypeDefOf.PositiveEvent);
        }
    }
}

public class ContactPot : TrapLikeBuilding
{
    public HashSet<Pawn> PawnsAlreadyHealed = new HashSet<Pawn>();

    protected override void CheckSpring(Pawn p)///这里是实现道具触发的地方
    {
        if (p.Faction == Faction.OfPlayer || p.HostFaction == Faction.OfPlayer)///检测
        {
            if (!PawnsAlreadyHealed.Add(p)) return;
            this.TryGetComp<CompHealing>().DoEffect(p);
            Messages.Message("Heal Success".Translate(), MessageTypeDefOf.PositiveEvent);///治疗并将p加入列表中
        }
    }

}





