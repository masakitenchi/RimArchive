using HarmonyLib;
using RimArchive.Components;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using CombatExtended;
using System.Linq;
using System.IO;
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
            rimarchive.PatchAll(Assembly.GetExecutingAssembly());
            if (ModsConfig.IsActive("ceteam.combatextended"))
            {
                /*rimarchive.Patch(AccessTools.Method(typeof(ArmorUtilityCE), "TryPenetrateArmor"), transpiler: new HarmonyMethod(typeof(HarmonyPatches), "ApplyArmorTranspiler_CE"));
                Debug.DbgMsg("RimArchive successfully patched ArmorUtilityCE.TryPenetrateArmor");*/
                Debug.DbgMsg("Currently Armor Reduction is not patched for CE");
            }
            else
            {
                rimarchive.Patch(AccessTools.Method(typeof(ArmorUtility), "ApplyArmor"), prefix: new HarmonyMethod(typeof(HarmonyPatches), "ApplyArmorPrefix"));
                Debug.DbgMsg("RimArchive successfully patched ArmorUtility.TryPenetrateArmor");
            }
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
            if (__instance.kindDef is StudentDef)
            {
                RimArchive.StudentDocument.Notify_StudentKilled(__instance);
                //然后应该让尸体在一阵光中消失
            }
            return;
        }

        //原版
        public static bool ApplyArmorPrefix(Pawn pawn,ref float armorRating)
        {
            Log.Message($"Before check:{armorRating}");
            if (pawn.health.hediffSet.hediffs.Exists(x => x.def == HediffDefOf.BA_ArmorReduction))
                armorRating *= (1 - pawn.health.hediffSet.hediffs.Find(x => x.def == HediffDefOf.BA_ArmorReduction).TryGetComp<HediffComp_ArmorReduction>().armorReduction);
            Log.Message($"After check:{armorRating}");
            return true;
        }

        //CE
        public static IEnumerable<CodeInstruction> ApplyArmorTranspiler_CE(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            return instructions;
        }

        /*public static void TryAdjustArmorRating(Pawn p, ref float armorRating)
        {
            if (!p.health.hediffSet.HasHediff(HediffDefOf.BA_ArmorReduction))
                return;
            armorRating *= p.health.hediffSet.hediffs.Where(x => x.def == HediffDefOf.BA_ArmorReduction).First().TryGetComp<HediffComp_ArmorReduction>().armorReduction;
        }*/
    }
}
