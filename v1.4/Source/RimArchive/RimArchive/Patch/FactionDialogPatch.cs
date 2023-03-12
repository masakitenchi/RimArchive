using RimWorld;
using HarmonyLib;
using Verse;

namespace RimArchive.Patch
{
    public class FactionDialogPatch
    {

        [HarmonyPatch(typeof(FactionDialogMaker),nameof(FactionDialogMaker.FactionDialogFor))]
        [HarmonyPostfix]
        public static void FactionDialogPostfix(ref DiaNode __result, Pawn negotiator, Faction faction)
        {
            if (faction.def.defName == RimArchiveWorldComponent.Shale)
            {
                RAFaction DialogRequest = new RAFaction(negotiator, faction, __result);
                __result.options.Insert(0, DialogRequest.CreateInitialDiaMenu());
            }
        }
    }
}
