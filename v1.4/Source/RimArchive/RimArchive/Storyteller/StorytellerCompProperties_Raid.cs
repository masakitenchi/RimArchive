using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RimArchive;

public class StorytellerCompProperties_Raid : StorytellerCompProperties
{
    [DefaultFloatRange(0.2f, 0.5f)]
    public FloatRange pointFactor;
    public float minIntervalDays = 0.5f;


    public StorytellerCompProperties_Raid() => this.compClass = typeof(StorytellerComp_Raid);
}


public class StorytellerComp_Raid : StorytellerComp
{
    public StorytellerCompProperties_Raid Props => this.props as StorytellerCompProperties_Raid;

    private static ThreatsGeneratorParams parms = new ThreatsGeneratorParams()
    {
        allowedThreats = AllowedThreatsGeneratorThreats.Raids,
        randSeed = Rand.Int,
        onDays = 5.0f,
        offDays = 3.0f,
        minSpacingDays = 0.5f,
        numIncidentsRange = new FloatRange(5.0f, 10.0f),
        minThreatPoints = StorytellerUtility.GlobalPointsMin()
    };

    public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
    {
        if(target is Map)
        {
            parms.currentThreatPointsFactor = Props.pointFactor.RandomInRange;
            parms.minSpacingDays = Props.minIntervalDays;
            foreach (var incident in ThreatsGenerator.MakeIntervalIncidents(parms, target, (target as Map)?.generationTick ?? 0))
            {
                incident.source = this;
                incident.parms.raidArrivalMode = PawnsArrivalModeDefOf.RandomDrop;
                yield return incident;
            }
        }
    }
}