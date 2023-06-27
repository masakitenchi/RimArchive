using System;
using System.Text;

namespace RimArchive;

/// <summary>
/// Use bossDamageFactor instead of bossCount
/// </summary>
public class RaidGroupWave
{
    public string label;
    /// <summary>
    /// The HP multiplier of boss
    /// </summary>
    [Obsolete]
    public float bossDamageFactor = 1f;
    /// <summary>
    /// The mobs coming along with boss
    /// </summary>
    public List<PawnKindDefCount> escorts = new List<PawnKindDefCount>();
    /// <summary>
    /// This overrides the setting in RaidDef
    /// </summary>
    public BossDef bossOverride;
    /// <summary>
    /// The apparel that boss carries
    /// </summary>
    public List<ThingDef> bossApparel;
    /// <summary>
    /// Hediffs to add to boss
    /// </summary>
    public List<HediffDef> hediffsToApply;


    public string GetWaveDescription(int wave)
    {
        StringBuilder sb = new StringBuilder();
        if (label is not null) sb.AppendLine(label);
        sb.AppendLine("BossHPMultiplier".Translate(HediffDefOf.BA_BossDamageReduction.stages[wave].statFactors.First().ToStringAsFactor));
        if (!this.escorts.NullOrEmpty())
        {
            sb.AppendLine("Escortees".Translate());
            foreach (var escortee in this.escorts)
            {
                sb.AppendLine("-" + escortee.kindDef.LabelCap + "x" + escortee.count);
            }
        }
        return sb.ToString();
    }
}
