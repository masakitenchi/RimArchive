using CombatExtended;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RimArchive;

[HarmonyPatch]
public class Harmony_CE
{

    public static void PartialStatPostfix_CE(ref float __result, Apparel apparel)
    {
        Pawn pawn = apparel.Wearer;
        __result *= pawn.health.hediffSet.hediffs.Exists(x => x.def == HediffDefOf.BA_ArmorReduction) ? (1 - pawn.health.hediffSet.hediffs.Find(x => x.def == HediffDefOf.BA_ArmorReduction).TryGetComp<HediffComp_ArmorReduction>().armorReduction) : 1;
    }

}
