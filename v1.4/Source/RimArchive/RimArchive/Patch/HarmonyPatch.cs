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
            /*if (ModsConfig.IsActive("ceteam.combatextended"))
            {
                MethodInfo PartialStat = AccessTools.Method(typeof(ArmorUtilityCE), "PartialStat", new System.Type[] { typeof(Pawn), typeof(StatDef), typeof(BodyPartRecord), typeof(float), typeof(float) });
                if(PartialStat == null)
                {
                    Debug.DbgErr("CE had changed ArmorUtilityCE.PartialStat. Please Contact mod Author");
                    return;
                }
                rimarchive.Patch(PartialStat, postfix: new HarmonyMethod(typeof(HarmonyPatches), "PartialStatPostfix_CE"));
                Debug.DbgMsg("RimArchive successfully patched ArmorUtilityCE.TryPenetrateArmor");
                //Debug.DbgMsg("Currently Armor Reduction is not patched for CE");
            }
            else*/
            {
                rimarchive.Patch(AccessTools.Method(typeof(ArmorUtility), "ApplyArmor"), transpiler: new HarmonyMethod(typeof(HarmonyPatches), "ApplyArmorTranspiler"));
                Debug.DbgMsg("RimArchive successfully patched ArmorUtility.ApplyArmor");
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Pawn))]
        [HarmonyPatch("Kill")]
        public static bool KillPrefix(Pawn __instance)
        {
            if(__instance.kindDef is StudentDef)
            {
                if(RimArchive.StudentDocument.Notify_StudentKilled(__instance))
                {
                    Debug.DbgMsg($"Successfully documented student\nDead? {__instance.health.Dead}");
                    __instance.DeSpawn();
                    return false;
                }
            }
            return true;
        }
/*
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pawn))]
        [HarmonyPatch("Kill")]
        public static void KillPostfix(Pawn __instance, ref bool __state)
        {
            if (__state)
            {
                //RimArchive.StudentDocument.Notify_StudentKilled(__instance);
                //然后应该让尸体在一阵光中消失
                __instance.Corpse.Destroy();
            }
            return;
        }
*/
        /*[HarmonyPrefix]
        [HarmonyPatch(typeof(Pawn))]
        [HarmonyPatch("DropAndForbidEverything")]
        public static bool DropAndForbidEverything_Postfix(Pawn __instance)
        {
            if(__instance.kindDef is StudentDef)
            {
                __instance.equipment.DestroyAllEquipment();
                __instance.apparel.DestroyAll();
                return false;
            }
            return true;
        }*/
        //原版
        /*public static bool ApplyArmorPrefix(Pawn pawn,ref float armorRating)
        {
            Log.Message($"Before check:{armorRating}");
            if (pawn.health.hediffSet.hediffs.Exists(x => x.def == HediffDefOf.BA_ArmorReduction))
                armorRating *= (1 - pawn.health.hediffSet.hediffs.Find(x => x.def == HediffDefOf.BA_ArmorReduction).TryGetComp<HediffComp_ArmorReduction>().armorReduction);
            Log.Message($"After check:{armorRating}");
            return true;
        }*/

        public static IEnumerable<CodeInstruction> ApplyArmorTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            MethodInfo method = AccessTools.Method(typeof(HarmonyPatches), "AdjustArmorRating");
            yield return new CodeInstruction(OpCodes.Ldarg_S, 5);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Call, method);
            yield return new CodeInstruction(OpCodes.Starg, 2);
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;
            }
        }
        //CE
        public static void PartialStatPostfix_CE(ref float __result, Pawn pawn)
        {
            __result *= pawn.health.hediffSet.hediffs.Exists(x => x.def == HediffDefOf.BA_ArmorReduction) ? (1 - pawn.health.hediffSet.hediffs.Find(x => x.def == HediffDefOf.BA_ArmorReduction).TryGetComp<HediffComp_ArmorReduction>().armorReduction) : 1;
        }

        public static float AdjustArmorRating(Pawn p, float armorRating)
        {
            if (!p.health.hediffSet.HasHediff(HediffDefOf.BA_ArmorReduction))
                return armorRating;
            return armorRating *= (1 - p.health.hediffSet.hediffs.Where(x => x.def == HediffDefOf.BA_ArmorReduction).First().TryGetComp<HediffComp_ArmorReduction>().armorReduction);
        }
    }
}
