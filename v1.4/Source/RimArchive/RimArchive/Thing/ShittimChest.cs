using RimArchive.WorldComponents;

namespace RimArchive;

public class Building_ShittimChest : Building_CommsConsole
{
    public IEnumerable<ICommunicable> GetCommTargetSchale(Pawn myPawn) =>

        from x in Find.FactionManager.AllFactionsVisibleInViewOrder
        where x.def.defName == RimArchiveWorldComponent.Shale
        select x;

    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn pawn)
    {
        Building_CommsConsole Building_ShittimChest = (Building_CommsConsole)this;
        FloatMenuOption option = GetCommTargetSchale(pawn).FirstOrDefault()?.CommFloatMenuOption(Building_ShittimChest, pawn);
        if (option != null)
        {
            yield return option;
        }
    }
}

