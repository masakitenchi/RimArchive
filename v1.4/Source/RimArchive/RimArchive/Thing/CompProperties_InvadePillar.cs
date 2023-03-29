using RimWorld;
using Verse;
using System.Collections.Generic;

namespace RimArchive;

public class CompProperties_InvadePillar : CompProperties
{
    public float InitialRadius = 0f;
    public List<HediffDef> applyHediffs;
    public int upgradeInterval = 0;
    public CompProperties_InvadePillar() => compClass = typeof(CompInvadePillar);
}
