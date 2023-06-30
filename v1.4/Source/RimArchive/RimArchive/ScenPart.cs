using UnityEngine;
using AlienRace;
using System.Diagnostics;
using System;
using static HarmonyLib.Code;

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
        race ??= PossibleRaces().RandomElement();
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
        p.genes.ClearXenogenes();
        foreach (var gene in p.genes.Xenotype.AllGenes)
            p.genes.AddGene(gene, false);
    }

    private static IEnumerable<ThingDef> PossibleRaces()
    {
        if (ModLister.HasActiveModWithName("Humanoid Alien Races") || ModsConfig.IsActive("erdelf.humanoidalienraces"))
            return DefDatabase<ThingDef_AlienRace>.AllDefs.
                Where(x =>  x.race?.Humanlike ?? false);
        return DefDatabase<ThingDef>.AllDefs.Where(x => x.race?.Humanlike ?? false);
    }
}


public class ScenPart_FixedStartingPawns : ScenPart_ConfigPage_ConfigureStartingPawnsBase
{
    public int pawnCount = 3;
    [MustTranslate]
    public string customSummary;

    //Count默认为1
    public List<PawnKindCount> possibleKindDefs = new List<PawnKindCount>();

    protected override int TotalPawnCount => possibleKindDefs.Sum(x => x.count);

    public override string Summary(Scenario scen) => this.customSummary ?? (string)"ScenPart_StartWithCertainColonists".Translate();

    public IEnumerable<PawnKindDef> availableDefs => DefDatabase<PawnKindDef>.AllDefs.Where(x => x.RaceProps.Humanlike && x.defaultFactionType != null && x.defaultFactionType.isPlayer);
    protected override void GenerateStartingPawns()
    {
        Log.Message("Generating Starting Pawns...");
        var num = 0;
        StartingPawnUtility.ClearAllStartingPawns();
        for(int i = 0; i< this.possibleKindDefs.Count; i++)
        {
            StartingPawnUtility.SetGenerationRequest(i, new PawnGenerationRequest(this.possibleKindDefs[i].kindDef));
            Pawn p = StartingPawnUtility.NewGeneratedStartingPawn(i);
            if (this.possibleKindDefs[i].kindDef is StudentDef s)
            {
                StudentGenerationUtility.PostGen(p, s);
            }
            StartingPawnUtility.AddNewPawn(i);
            ++num;
            Log.Message($"Added {p} to Starting Pawn");
        }
        while(num < TotalPawnCount && !StartingPawnUtility.WorkTypeRequirementsSatisfied())
        {
            Log.Message("Adding Extra Pawns to Starting Pawn");
            StartingPawnUtility.AddNewPawn();
            ++num;
        }
        Log.Message($"Generated {num} pawns");
    }


    public override void PostIdeoChosen()
    {
        Find.GameInitData.startingPawnCount = TotalPawnCount;
        base.PostIdeoChosen();
    }
    public override void DoEditInterface(Listing_ScenEdit listing)
    {
        Rect rect = listing.GetScenPartRect(this, RowHeight * 4f) with
        {
            height = RowHeight
        };
        for (int i = 0; i < possibleKindDefs.Count; i++)
        {
            PawnKindCount kindCount = possibleKindDefs[i];
            Rect MenuRect = new Rect(rect);
            MenuRect.xMax -= RowHeight;
            if (Widgets.ButtonText(MenuRect, kindCount.kindDef.LabelCap))
            {
                /*List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach(PawnKindDef def in availableDefs)
                {
                    PawnKindDef kindDef1 = def;
                    options.Add(new FloatMenuOption(kindDef1.LabelCap, (() => kindDef = kindDef1)));
                }
                if (options.Any())
                    Find.WindowStack.Add(new FloatMenu(options));*/
                FloatMenuUtility.MakeMenu(availableDefs, x => (string)x.LabelCap, delegate (PawnKindDef x)
                {
                    Log.Message($"Trying to set {kindCount.kindDef.defName}@{i} to {x.defName}");
                    return () => kindCount.kindDef = x;
                });
            }
            if (Widgets.ButtonImage(rect with
            {
                xMin = MenuRect.xMax
            }, TexButton.DeleteX))
            {
                this.possibleKindDefs.RemoveAt(i);
                return;
            }
            rect.y += RowHeight;
        }
        if (Widgets.ButtonText(rect, "Add"))
        {
            possibleKindDefs.Add(new PawnKindCount()
            {
                kindDef = PawnKindDefOf.Colonist
            });
        }
    }

}