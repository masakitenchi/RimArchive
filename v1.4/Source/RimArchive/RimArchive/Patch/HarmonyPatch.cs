using HarmonyLib;
using RimArchive.Components;
using RimWorld;
using System.Reflection;
using Verse;

namespace RimArchive
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony rimarchive = new Harmony("com.regex.RimArchive");
            rimarchive.Patch(AccessTools.Method(typeof(FactionDialogMaker), "FactionDialogFor"), postfix: new HarmonyMethod(typeof(HarmonyPatches), "FactionDialogPostfix"));
            rimarchive.PatchAll();
            //FactionDialogPatch.Patch();
        }

        [HarmonyPatch(typeof(FactionDialogMaker), "FactionDialogFor")]
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
