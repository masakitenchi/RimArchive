using UnityEngine;
using AlienRace;

namespace RimArchive;

public class ScenPart_ForcedRace : ScenPart_PawnModifier
{
    public ThingDef race;
    protected override void ModifyNewPawn(Pawn p)
    {
        if(p.def != this.race)
            p.def = this.race;
    }

    public override void DoEditInterface(Listing_ScenEdit listing)
    {
        Rect scenPartRect = listing.GetScenPartRect(this, RowHeight * 3.0f + 31.0f);
        if(Widgets.ButtonText(scenPartRect.TopPartPixels(RowHeight), (string) race.LabelCap))
        {
            FloatMenuUtility.MakeMenu<ThingDef>(PossibleRaces(), x => (string)x.LabelCap, x => () => { this.race = x; });
        }
    }

    protected override void ModifyPawnPostGenerate(Pawn p, bool redressed)
    {
        p.story.hairDef = HairDefOf.Bald;
        p.style.beardDef = BeardDefOf.NoBeard;
        p.genes.SetXenotype((p.def as ThingDef_AlienRace).alienRace.raceRestriction.xenotypeList.First());
    }

    private static IEnumerable<ThingDef> PossibleRaces()
    {
        if (ModLister.HasActiveModWithName("Humanoid Alien Races") || ModsConfig.IsActive("erdelf.humanoidalienraces"))
            return DefDatabase<ThingDef_AlienRace>.AllDefs.
                Where(x =>  x.race is not null && 
                            x.race.IsFlesh && 
                            x.race.intelligence == Intelligence.Humanlike);
        return DefDatabase<ThingDef>.AllDefs.Where(x => x.race is not null &&
                                                        x.race.IsFlesh &&
                                                        x.race.intelligence == Intelligence.Humanlike);
    }
}
