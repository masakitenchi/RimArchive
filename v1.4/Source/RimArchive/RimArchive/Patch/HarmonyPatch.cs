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
            MethodInfo ce = AccessTools.Method("CombatExtended.CE_Utility:PartialStat", new System.Type[] { typeof(Apparel), typeof(StatDef), typeof(BodyPartRecord) });
            MethodInfo vanilla = AccessTools.Method(typeof(ArmorUtility), "ApplyArmor");
            //Debug.DbgMsg($"ce :{ce}\n vanilla:{vanilla}");
            if (ce != null)
            {
                //Debug.DbgErr("CE had changed ArmorUtilityCE.PartialStat. Please Contact mod Author");
                rimarchive.Patch(ce, postfix: new HarmonyMethod(typeof(Harmony_CE), "PartialStatPostfix_CE"));
                Debug.DbgMsg("RimArchive successfully patched CombatExtended.CE_Utility:PartialStat(this Apparel apparel, StatDef stat, BodyPartRecord part)");
                //Debug.DbgMsg("Currently Armor Reduction is not patched for CE");
            }
            else if (vanilla != null)
            {
                rimarchive.Patch(AccessTools.Method(typeof(ArmorUtility), "ApplyArmor"), transpiler: new HarmonyMethod(typeof(HarmonyPatches), "ApplyArmorTranspiler"));
                Debug.DbgMsg("successfully patched ArmorUtility.ApplyArmor");
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
            if (__instance.kindDef is StudentDef)
            {
                if (RimArchive.StudentDocument.Notify_StudentKilled(__instance))
                {
                    //Debug.DbgMsg($"Successfully documented student\nDead? {__instance.health.Dead}");
                    __instance.apparel.WornApparel.Select(x => x.HitPoints = x.MaxHitPoints);
                    __instance.equipment.AllEquipmentListForReading.Select(x => x.HitPoints = x.MaxHitPoints);
                    __instance.health.hediffSet.hediffs.RemoveAll(x => x.def.isBad || x.def.IsAddiction); __instance.DeSpawn();
                    return false;
                }
            }
            return true;
        }


        /*要点：
         * 1.这个条到底怎么搞，是纯粹的伤害黄条还是带上CC的控制条
         * 2.完全禁用CC的特性决定了必须是第一个执行的Prefix
         * 3.通货膨胀了的话如何保证相对的平衡
         * 
         * 目前特征：
         * 柱子只看晕眩伤和EMP，Boss看所有(同时模拟Groggy Gauge)
         * 换算率：1点伤害 = 30Tick晕眩(游戏内定义)
         * 晕眩时条不涨，触发晕眩的那一瞬间条已经清空
         */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ThingWithComps), nameof(ThingWithComps.PreApplyDamage))]
        [HarmonyPriority(Priority.First)]
        public static bool Prefix_StunHandler(ThingWithComps __instance, ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
            float duration = 0f;
            switch (__instance)
            {
                case Pawn pawn:
                    if (pawn.def.HasComp(typeof(CompStunHandler)))
                    {
                        if (!pawn.stances.stunner.Stunned && !pawn.GetComp<CompStunHandler>().TryAddStunDuration(GenTicks.TicksToSeconds((int)(dinfo.Amount * StunHandler.StunDurationTicksPerDamage)), out duration))
                        {
                            pawn.stances.stunner.StunFor(GenTicks.SecondsToTicks(duration), dinfo.Instigator);
                        }
                        else
                        {
                            absorbed = true;
                            dinfo.SetAmount(0f);
                        }
                    }
                    break;
                case Building building:
                    if (building.def.HasComp(typeof(CompStunHandler)) && (dinfo.Def == DamageDefOf.EMP || dinfo.Def == DamageDefOf.Stun) && !building.GetComp<CompStunHandler>().TryAddStunDuration(GenTicks.TicksToSeconds((int)(dinfo.Amount * StunHandler.StunDurationTicksPerDamage)), out duration))
                    {
                        building.GetComp<CompInvadePillar>();
                        building.TakeDamage(new DamageInfo(DamageDefOf.Bomb, 50));
                    }
                    break;
            }
            return true;
        }

        #region Armor Reduction
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

        public static float AdjustArmorRating(Pawn p, float armorRating)
        {
            if (!p.health.hediffSet.HasHediff(HediffDefOf.BA_ArmorReduction))
                return armorRating;
            return armorRating *= (1 - p.health.hediffSet.hediffs.Where(x => x.def == HediffDefOf.BA_ArmorReduction).First().TryGetComp<HediffComp_ArmorReduction>().armorReduction);
        }
        #endregion

    }
}
