using RimWorld;
using System.Collections.Generic;
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
    /// The apparel that boss carries
    /// </summary>
    public List<ThingDef> bossApparel;
    /// <summary>
    /// The mobs coming along with boss
    /// </summary>
    public List<PawnKindDefCount> escorts = new List<PawnKindDefCount>();
    /// <summary>
    /// This overrides the setting in RaidDef
    /// </summary>
    public BossDef? bossOverride;
}
