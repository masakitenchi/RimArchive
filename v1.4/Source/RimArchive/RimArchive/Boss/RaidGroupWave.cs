using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimArchive;

/// <summary>
/// Use bossHPMultiplier instead of bossCount
/// </summary>
public class RaidGroupWave
{
    /// <summary>
    /// The HP multiplier of boss
    /// </summary>
    public float bossHPMultiplier = 1f;
    /// <summary>
    /// The mobs coming along with boss
    /// </summary>
    public List<PawnKindDefCount> escorts = new List<PawnKindDefCount>();
#nullable enable
    /// <summary>
    /// This overrides the setting in RaidDef
    /// </summary>
    public BossDef? bossOverride;
    /// <summary>
    /// The apparel that boss carries
    /// </summary>
    public List<ThingDef>? bossApparel;


    public string GetWaveDescription()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("BossHPMultiplier".Translate(this.bossHPMultiplier.ToStringPercent()));
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
