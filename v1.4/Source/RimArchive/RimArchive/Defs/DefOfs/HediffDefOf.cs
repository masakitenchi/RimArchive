using Verse;
using RimWorld;

namespace RimArchive;

[DefOf]
[StaticConstructorOnStartup]
public static class HediffDefOf
{
    public static HediffDef BA_ArmorReduction;

    static HediffDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
    }
}
