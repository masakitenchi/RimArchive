using RimWorld;
using System.Collections.Generic;
using Verse;
using HarmonyLib;


namespace RimArchive;

[HarmonyPatch]
public static class ReversePatch
{
    [HarmonyReversePatch]
    public static void Refuelable()
    {

    }
}

public class CompProperties_FuelableTickRare : CompProperties_Refuelable
{


    public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
    {
        return base.ConfigErrors(parentDef);
    }
}
