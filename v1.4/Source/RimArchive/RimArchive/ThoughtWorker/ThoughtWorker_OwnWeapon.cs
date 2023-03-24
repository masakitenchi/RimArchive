using RimWorld;
using Verse;

namespace RimArchive;

public class ThoughtWorker_OwnWeapon : ThoughtWorker
{
    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        return p.equipment.AllEquipmentListForReading.Exists(x => x.def == (p.kindDef as StudentDef).ownWeapon);
    }
}
    