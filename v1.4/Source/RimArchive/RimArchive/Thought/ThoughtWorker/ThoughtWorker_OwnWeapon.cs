namespace RimArchive;

public class ThoughtWorker_OwnWeapon : ThoughtWorker
{
    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        if (p.kindDef is StudentDef a)
            return p.equipment.AllEquipmentListForReading.Exists(x => x.def == a.ownWeapon);
        return ThoughtState.Inactive;
    }
}
