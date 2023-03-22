using RimWorld;
using Verse;

namespace RimArchive;

public class HediffComp_ArmorReduction : HediffComp
{
    public HediffCompProperties_ArmorReduction Props => (HediffCompProperties_ArmorReduction)props;

    public float armorReduction = 0.1f;

    public override string CompTipStringExtra
    {
        get
        {
            return base.CompTipStringExtra + "CurrentArmorReduction:".Translate() + armorReduction.ToStringPercent();
        }
    }
    public HediffComp_ArmorReduction()
    {
    }

}
