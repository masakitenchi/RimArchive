using HarmonyLib;
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
            new Harmony("com.regex.RimArchive").PatchAll(Assembly.GetExecutingAssembly());
            //FactionDialogPatch.Patch();
        }
    }
}
