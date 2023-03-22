using RimWorld;
using Verse;

namespace RimArchive;

public class HediffCompProperties_ArmorReduction : HediffCompProperties
{
    public float baseAmount = 0.10f;
    public HediffCompProperties_ArmorReduction()
    {
        compClass = typeof(HediffComp_ArmorReduction);
    }
}
