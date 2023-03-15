using HarmonyLib;
using RimArchive.Components;
using RimWorld;
using System.Reflection;
using Verse;
#pragma warning disable CS1591

namespace RimArchive
{
    [StaticConstructorOnStartup]
    [HarmonyPatch]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony rimarchive = new Harmony("com.regex.RimArchive");
            //rimarchive.Patch(AccessTools.Method(typeof(FactionDialogMaker), "FactionDialogFor"), postfix: new HarmonyMethod(typeof(HarmonyPatches), "FactionDialogPostfix"));
            rimarchive.PatchAll(Assembly.GetExecutingAssembly());
            //FactionDialogPatch.Patch();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FactionDialogMaker))]
        [HarmonyPatch("FactionDialogFor")]
        public static void FactionDialogPostfix(ref DiaNode __result, Pawn negotiator, Faction faction)
        {
            if (faction.def.defName == RimArchiveWorldComponent.Shale)
            {
                RAFaction DialogRequest = new RAFaction(negotiator, faction, __result);
                __result.options.Insert(0, DialogRequest.CreateInitialDiaMenu());
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pawn))]
        [HarmonyPatch("Kill")]
        public static void KillPostfix(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            if(__instance.kindDef is StudentDef)
            {
                RimArchive.StudentDocument.Notify_StudentKilled(ref __instance);
            }
            return;
        }
    }
}
