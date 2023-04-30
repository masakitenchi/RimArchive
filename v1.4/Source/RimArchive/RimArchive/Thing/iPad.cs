using RimWorld;
using Verse;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using RimArchive.WorldComponents;

namespace RimArchive;

public class iPad : Building_CommsConsole
{
    new public IEnumerable<ICommunicable> GetCommTargets(Pawn myPawn) => 
        from x in Find.FactionManager.AllFactionsVisibleInViewOrder 
        where x.def.defName == RimArchiveWorldComponent.Shale
        select x;
    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
    {
        Building_CommsConsole iPad = (Building_CommsConsole)this;
        FloatMenuOption option = GetCommTargets(selPawn).FirstOrDefault()?.CommFloatMenuOption(iPad, selPawn);
        if(option != null)
        {
            yield return option;
        }
    }
}
